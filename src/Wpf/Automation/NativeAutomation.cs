using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace RepeatStudio.Automation {
class MacroEvent { public long Time; public int Message; public int X,Y,Data,Flags; public bool Keyboard; }
partial class AutomationEngine {
 delegate IntPtr HookProc(int code,IntPtr message,IntPtr data);
 HookProc mouseProc,keyProc; IntPtr mouseHook,keyHook;
 readonly List<MacroEvent> events=new List<MacroEvent>();
 readonly HashSet<int> keys=new HashSet<int>(); readonly HashSet<uint> buttons=new HashSet<uint>();
 readonly Stopwatch watch=new Stopwatch(); bool recording,playing; volatile bool stop;
    void Replay(MacroEvent a){if(a.Keyboard){bool up=a.Message==0x101||a.Message==0x105;keybd_event((byte)a.Data,0,(uint)((up?2:0)|((a.Flags&1)!=0?1:0)),UIntPtr.Zero);if(up)keys.Remove(a.Data);else keys.Add(a.Data);return;}SetCursorPos(a.X,a.Y);uint f=0;switch(a.Message){case 0x201:f=2;break;case 0x202:f=4;break;case 0x204:f=8;break;case 0x205:f=16;break;case 0x207:f=32;break;case 0x208:f=64;break;case 0x20A:f=0x800;break;case 0x20E:f=0x1000;break;case 0x20B:f=0x80;break;case 0x20C:f=0x100;break;}uint data=unchecked((uint)(a.Data>>16));if(f!=0)mouse_event(f,0,0,data,UIntPtr.Zero);if(f==2||f==8||f==32)buttons.Add(f);if(f==4||f==16||f==64)buttons.Remove(f/2);if(f==0x80)buttons.Add(data==1?0x10001u:0x10002u);if(f==0x100)buttons.Remove(data==1?0x10001u:0x10002u);}
    void ReleaseInputs(){foreach(int k in keys)keybd_event((byte)k,0,2,UIntPtr.Zero);keys.Clear();foreach(uint b in buttons)mouse_event(b>=0x10000?0x100:b*2,0,0,b>=0x10000?b-0x10000:0,UIntPtr.Zero);buttons.Clear();}
    [StructLayout(LayoutKind.Sequential)]struct MouseData{public int x,y,data,flags,time;public UIntPtr extra;}
    [StructLayout(LayoutKind.Sequential)]struct KeyData{public int vk,scan,flags,time;public UIntPtr extra;}
    [DllImport("user32.dll")]static extern IntPtr SetWindowsHookEx(int id,HookProc proc,IntPtr module,uint thread);
    [DllImport("user32.dll")]static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")]static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr msg,IntPtr data);
    [DllImport("kernel32.dll",CharSet=CharSet.Auto)]static extern IntPtr GetModuleHandle(string name);
    [DllImport("user32.dll")]static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
    [DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr window,int id);
    internal bool SuppressNativeInput;
    internal Action<string,uint,uint> InputObserved;
    internal Action<int,int> CursorObserved;
    bool SetCursorPos(int x,int y){CursorObserved?.Invoke(x,y);return SuppressNativeInput||NativeSetCursorPos(x,y);}
    [DllImport("user32.dll",EntryPoint="SetCursorPos")]static extern bool NativeSetCursorPos(int x,int y);
    void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra){InputObserved?.Invoke("mouse",flags,data);if(!SuppressNativeInput)NativeMouseEvent(flags,x,y,data,extra);}
    [DllImport("user32.dll",EntryPoint="mouse_event")]static extern void NativeMouseEvent(uint flags,uint x,uint y,uint data,UIntPtr extra);
    void keybd_event(byte key,byte scan,uint flags,UIntPtr extra){InputObserved?.Invoke("keyboard",key,flags);if(!SuppressNativeInput)NativeKeyEvent(key,scan,flags,extra);}
    [DllImport("user32.dll",EntryPoint="keybd_event")]static extern void NativeKeyEvent(byte key,byte scan,uint flags,UIntPtr extra);
}}