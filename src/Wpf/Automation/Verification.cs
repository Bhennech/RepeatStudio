using System;
using System.Diagnostics;
using System.Threading.Tasks;
namespace RepeatStudio.Automation {
partial class AutomationEngine {
 internal async Task Verify(){if(mouseHook==IntPtr.Zero||keyHook==IntPtr.Zero)throw new Exception("Input hooks not installed");
  ToggleRecord();if(!recording)throw new Exception("Record start failed");
  // Feed the exact native hook structures to exercise capture and the injected
  // input filter deterministically, without asking a person to type during CI.
  var memory=System.Runtime.InteropServices.Marshal.AllocHGlobal(64);
  try{System.Runtime.InteropServices.Marshal.StructureToPtr(new KeyData{vk=0x87},memory,false);Keyboard(0,new IntPtr(0x100),memory);await Task.Delay(40);Keyboard(0,new IntPtr(0x101),memory);
   System.Runtime.InteropServices.Marshal.StructureToPtr(new MouseData{x=20,y=20},memory,false);Mouse(0,new IntPtr(0x200),memory);
   int count=events.Count;System.Runtime.InteropServices.Marshal.StructureToPtr(new KeyData{vk=0x87,flags=0x10},memory,false);Keyboard(0,new IntPtr(0x100),memory);if(events.Count!=count||count<3||events[1].Time<30)throw new Exception("Capture timestamps or injection filter failed");
  }finally{System.Runtime.InteropServices.Marshal.FreeHGlobal(memory);}
  ToggleRecord();if(recording)throw new Exception("Record stop failed");events.Clear();
  // Replay harmless F24 input, checking measured timing and release of a key
  // intentionally left held by the recording. No external target is clicked.
  events.Add(new MacroEvent{Time=0,Keyboard=true,Message=0x100,Data=0x87});events.Add(new MacroEvent{Time=100,Keyboard=true,Message=0x101,Data=0x87});events.Add(new MacroEvent{Time=150,Keyboard=true,Message=0x100,Data=0x87});
  var clock=Stopwatch.StartNew();await Playback(1,1);if(playing||keys.Count!=0||clock.ElapsedMilliseconds<3100)throw new Exception("Timed playback or held key release failed");
  var pending=Playback(0,1);await Task.Delay(50);Stop();await pending;if(playing||keys.Count!=0||buttons.Count!=0)throw new Exception("Emergency playback cancellation failed");
  pending=Simple(10,0,0);await Task.Delay(50);Stop();await pending;if(playing)throw new Exception("Infinite click cancellation failed");
  Clear();if(events.Count!=0)throw new Exception("Clear failed");
 }
 internal async Task VerifyImageSearch(){
  var oldCapture=Capture;int down=0,up=0,cursorX=0,cursorY=0;
  InputObserved=(kind,flags,data)=>{if(kind=="mouse"&&flags==2)down++;if(kind=="mouse"&&flags==4)up++;};CursorObserved=(x,y)=>{cursorX=x;cursorY=y;};
  var monitor=System.Windows.Forms.Screen.AllScreens[0].Bounds;
  var area=new System.Drawing.Rectangle(monitor.X+20,monitor.Y+20,24,24);
  using(var template=new System.Drawing.Bitmap(3,3,System.Drawing.Imaging.PixelFormat.Format24bppRgb)){
   using(var g=System.Drawing.Graphics.FromImage(template))g.Clear(System.Drawing.Color.FromArgb(13,231,117));
   Capture=bounds=>{var frame=new System.Drawing.Bitmap(bounds.Width,bounds.Height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);using(var g=System.Drawing.Graphics.FromImage(frame)){g.Clear(System.Drawing.Color.Black);g.DrawImageUnscaled(template,5,5);}return frame;};
   try{
    await Search(template,area,null,()=>{});
    if(down!=1||up!=1||cursorX!=area.X+6||cursorY!=area.Y+6)throw new Exception("Image center matching or click dispatch failed");
    var point=new System.Drawing.Point(area.X+18,area.Y+18);await Search(template,area,point,()=>{});
    if(down!=2||up!=2||cursorX!=point.X||cursorY!=point.Y)throw new Exception("Fixed image click position failed");
    var clicks=new System.Collections.Generic.List<long>();var timer=Stopwatch.StartNew();
    InputObserved=(kind,flags,data)=>{if(kind=="mouse"&&flags==2){down++;clicks.Add(timer.ElapsedMilliseconds);}if(kind=="mouse"&&flags==4)up++;};
    await Search(template,area,null,()=>{},3,120);
    if(down!=5||up!=5||clicks.Count!=3||clicks[1]-clicks[0]<115||clicks[2]-clicks[1]<115)throw new Exception("Repeated image click count or frequency failed");
    int imageBefore=down;var repeatSearch=Search(template,area,null,()=>{},0,2000);var imageDeadline=Stopwatch.StartNew();
    while(down==imageBefore&&imageDeadline.ElapsedMilliseconds<6000)await Task.Delay(25);
    var cancellation=Stopwatch.StartNew();Stop();await repeatSearch;
    if(down!=imageBefore+1||playing||cancellation.ElapsedMilliseconds>500)throw new Exception("Infinite image search cancellation failed");
    down=2;up=2;
    await Simple(10,2,0);if(down!=4||up!=4)throw new Exception("Finite click count failed");
    int before=down;var infinite=Simple(10,0,0);var deadline=Stopwatch.StartNew();while(down==before&&deadline.ElapsedMilliseconds<6000)await Task.Delay(25);Stop();await infinite;if(down<=before||playing)throw new Exception("Infinite clicks or active emergency stop failed");
   }finally{Capture=oldCapture;InputObserved=null;CursorObserved=null;}
  }
 }
}}
