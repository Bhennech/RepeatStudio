using System;
using System.Runtime.InteropServices;
namespace RepeatStudio.Automation { partial class AutomationEngine {
    IntPtr Mouse(int code,IntPtr msg,IntPtr ptr){if(code>=0&&recording){ MouseData d=(MouseData)Marshal.PtrToStructure(ptr,typeof(MouseData)); if((d.flags&1)==0)events.Add(new MacroEvent{Time=watch.ElapsedMilliseconds,Message=msg.ToInt32(),X=d.x,Y=d.y,Data=d.data}); }return CallNextHookEx(mouseHook,code,msg,ptr);}
    IntPtr Keyboard(int code,IntPtr msg,IntPtr ptr){if(code>=0&&recording){KeyData d=(KeyData)Marshal.PtrToStructure(ptr,typeof(KeyData));if(d.vk<0x77||d.vk>0x79){if((d.flags&0x10)==0)events.Add(new MacroEvent{Time=watch.ElapsedMilliseconds,Keyboard=true,Message=msg.ToInt32(),Data=d.vk,Flags=d.flags});}}return CallNextHookEx(keyHook,code,msg,ptr);}
}}