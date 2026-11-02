using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace RepeatStudio.Automation { partial class AutomationEngine {
    static byte[] Pixels(Bitmap bitmap,out int stride) {
        var data=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),System.Drawing.Imaging.ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        try {stride=data.Stride;byte[] bytes=new byte[stride*bitmap.Height];Marshal.Copy(data.Scan0,bytes,0,bytes.Length);return bytes;}
        finally{bitmap.UnlockBits(data);}
    }
    internal Func<Rectangle,Bitmap> Capture=CaptureDesktop;
    static Bitmap CaptureDesktop(Rectangle bounds){var shot=new Bitmap(bounds.Width,bounds.Height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);try{using(var g=Graphics.FromImage(shot))g.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size);return shot;}catch{shot.Dispose();throw;}}
    Point? FindOnScreens(Bitmap template, Rectangle? selectedArea) {
        int ts;byte[] t=Pixels(template,out ts);
        foreach(Screen screen in Screen.AllScreens){
            if(stop)return null;
            Rectangle bounds=selectedArea.HasValue?Rectangle.Intersect(screen.Bounds,selectedArea.Value):screen.Bounds;
            if(template.Width>bounds.Width||template.Height>bounds.Height)continue;
            using(Bitmap shot=Capture(bounds)){
                int ss;byte[] pixels=Pixels(shot,out ss);
                for(int y=0;y<=shot.Height-template.Height;y++){
                    if(stop)return null;
                    for(int x=0;x<=shot.Width-template.Width;x++){
                        bool match=true;
                        for(int gy=0;gy<5&&match;gy++)for(int gx=0;gx<5&&match;gx++){
                            int tx=gx*(template.Width-1)/4,ty=gy*(template.Height-1)/4;
                            int a=ty*ts+tx*3,b=(y+ty)*ss+(x+tx)*3;
                            for(int c=0;c<3;c++)if(Math.Abs(t[a+c]-pixels[b+c])>12){match=false;break;}
                        }
                        for(int ty=0;ty<template.Height&&match;ty++){
                            if(stop)return null;
                            for(int tx=0;tx<template.Width*3;tx++)
                                if(Math.Abs(t[ty*ts+tx]-pixels[(y+ty)*ss+x*3+tx])>12){match=false;break;}
                        }
                        if(match)return new Point(bounds.X+x+template.Width/2,bounds.Y+y+template.Height/2);
                    }
                }
            }
        }
        return null;
    }

}}