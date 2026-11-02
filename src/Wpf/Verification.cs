using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace RepeatStudio {
public partial class MainWindow {
 async void OnLoaded(object sender,RoutedEventArgs e){var args=Environment.GetCommandLineArgs();if(args.Length<3)return;
  if(args[1]=="--screenshot"){await Task.Delay(1200);SaveScreenshot(args[2]);Close();}
  else if(args[1]=="--verify"){await Task.Delay(500);try{var folder=Path.GetFullPath(args[2]);Directory.CreateDirectory(folder);for(int i=0;i<3;i++){SelectPage(i);await Task.Delay(100);SaveScreenshot(Path.Combine(folder,"mode-"+i+".png"));}ToggleTabs(null,null);await Task.Delay(100);SaveScreenshot(Path.Combine(folder,"sidebar-hidden.png"));ToggleTabs(null,null);Width=1100;Height=800;await Task.Delay(100);SaveScreenshot(Path.Combine(folder,"resized.png"));Width=836;Height=745;SelectPage(0);await Task.Delay(100);engine.SuppressNativeInput=true;await engine.Verify();await VerifyImageSearch();await VerifyShortcuts();File.WriteAllText(Path.Combine(folder,"result.txt"),"PASS: all pages rendered; sidebar hidden and restored; window resized; native hooks installed; capture timestamps and injection filter; timed keyboard playback; held-input release; F8/F9/F10 native-message dispatch; infinite cancellation; image recognition on deterministic screen frames; finite mouse-button dispatch. Live desktop capture and input injection are unavailable in this sandbox (CopyFromScreen: invalid handle); physical shortcuts, hardware capture, actual playback, mixed-DPI selection and flicker remain unverified.");}catch(Exception ex){Directory.CreateDirectory(args[2]);File.WriteAllText(Path.Combine(args[2],"result.txt"),"FAIL: "+ex);}finally{engine.SuppressNativeInput=false;Close();}}
 }
 void SaveScreenshot(string path){UpdateLayout();var dpi=VisualTreeHelper.GetDpi(this);var bitmap=new RenderTargetBitmap((int)Math.Ceiling(ActualWidth*dpi.DpiScaleX),(int)Math.Ceiling(ActualHeight*dpi.DpiScaleY),dpi.PixelsPerInchX,dpi.PixelsPerInchY,PixelFormats.Pbgra32);bitmap.Render(this);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));var full=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(full));using(var file=File.Create(full))encoder.Save(file);}
 async Task VerifyShortcuts(){
  // Verify the WPF native-message route; physical global keys require an
  // interactive desktop, which the build sandbox does not expose.
  void Dispatch(int id){bool handled=false;WindowMessage(handle,0x312,new IntPtr(id),IntPtr.Zero,ref handled);if(!handled)throw new Exception("Hotkey message was not handled");}
  Dispatch(1);if(!engine.Recording)throw new Exception("F8 message start failed");Dispatch(1);if(engine.Recording)throw new Exception("F8 message stop failed");clickRepeats.Value=0;Dispatch(2);await Task.Delay(50);if(!engine.Playing)throw new Exception("F9 message start failed");Dispatch(3);await Task.Delay(100);if(engine.Playing)throw new Exception("F10 message stop failed");clickRepeats.Value=1;
 }
 async Task VerifyImageSearch(){await engine.VerifyImageSearch();}
}}
