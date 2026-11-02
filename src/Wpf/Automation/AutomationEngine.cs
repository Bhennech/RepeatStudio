using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
namespace RepeatStudio.Automation {
partial class AutomationEngine : IDisposable {
 public event Action<string> Status; public event Action Changed;
 public bool Recording => recording; public bool Playing => playing; public int Count => events.Count;
 public void Initialize(){mouseProc=Mouse;keyProc=Keyboard;mouseHook=SetWindowsHookEx(14,mouseProc,GetModuleHandle(null),0);keyHook=SetWindowsHookEx(13,keyProc,GetModuleHandle(null),0);if(mouseHook==IntPtr.Zero||keyHook==IntPtr.Zero)throw new InvalidOperationException("Input capture unavailable; restart Repeat Studio.");}
 void Report(string text){Status?.Invoke(text);}
 public void ToggleRecord(){if(playing)return;if(mouseHook==IntPtr.Zero||keyHook==IntPtr.Zero){Report("Input capture unavailable. Restart Repeat Studio.");return;}recording=!recording;if(recording){events.Clear();watch.Restart();Report("Recording — press F8 to finish");}else{watch.Stop();Report("Recording saved. Ready to replay.");}Changed?.Invoke();}
 public void Clear(){if(recording||playing)return;events.Clear();Changed?.Invoke();}
 public void Stop(){stop=true;if(recording)ToggleRecord();ReleaseInputs();}
 async Task<bool> Wait(long ms){var clock=Stopwatch.StartNew();long remaining;while(!stop&&(remaining=ms-clock.ElapsedMilliseconds)>0)await Task.Delay((int)Math.Min(remaining,25));return !stop;}
 async Task<bool> Countdown(){for(int n=3;n>0&&!stop;n--){Report("Starting in "+n+" — F10 to cancel");await Wait(1000);}return !stop;}
 async Task Run(Func<Task> action){if(recording||playing)return;playing=true;stop=false;Changed?.Invoke();try{if(await Countdown())await action();if(stop)Report("Stopped — inputs released");}catch(Exception ex){Report("Automation failed: "+ex.Message);}finally{ReleaseInputs();playing=false;Changed?.Invoke();}}
 public Task Simple(int interval,int total,int button){return Run(async()=>{uint down=button==1?8u:button==2?32u:button>=3?0x80u:2u;uint data=button>=3?(uint)(button-2):0;long completed=0;while(!stop&&(total==0||completed<total)){mouse_event(down,0,0,data,UIntPtr.Zero);mouse_event(down*2,0,0,data,UIntPtr.Zero);completed++;Report("Clicks: "+completed+(total==0?" / infinite":" / "+total)+" — F10 stops");if(total!=0&&completed>=total)break;if(!await Wait(interval))break;}if(!stop)Report("Clicking complete");});}
 public Task Playback(int total,double speed){if(events.Count==0)return Task.CompletedTask;return Run(async()=>{for(long r=0;(total==0||r<total)&&!stop;r++){Report("Playing "+(r+1)+" / "+(total==0?"infinite":total.ToString())+" — F10 to stop");var clock=Stopwatch.StartNew();foreach(var a in events){if(!await Wait((long)Math.Round(a.Time/speed)-clock.ElapsedMilliseconds))break;Replay(a);}ReleaseInputs();if(total==0||r+1<total)await Wait(250);}if(!stop)Report("Playback complete");});}
 public Task Search(Bitmap template,Rectangle? area,Point? click,Action minimize){return Run(async()=>{minimize();if(!await Wait(350))return;Report("Searching until image is found — F10 cancels");while(!stop){Point? match=await Task.Run(()=>FindOnScreens(template,area));if(stop)break;if(match.HasValue){Point p=click??match.Value;SetCursorPos(p.X,p.Y);mouse_event(2,0,0,0,UIntPtr.Zero);mouse_event(4,0,0,0,UIntPtr.Zero);Report("Image found and clicked.");return;}await Wait(500);}});}
 public void Dispose(){Stop();UnhookWindowsHookEx(mouseHook);UnhookWindowsHookEx(keyHook);}
}}
