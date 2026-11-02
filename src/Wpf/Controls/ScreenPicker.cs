using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Drawing=System.Drawing;
namespace RepeatStudio.Controls {
// A lightweight WPF overlay; native pixels are obtained through PointToScreen,
// including monitors with negative origins or a different DPI.
public sealed class ScreenPicker : Window {
 readonly bool pointMode; readonly Canvas canvas=new Canvas(); readonly Rectangle outline=new Rectangle { Stroke=Brushes.Cyan,StrokeThickness=3,Fill=Brushes.Transparent };
 Point start; bool dragging; public Drawing.Rectangle Selection {get;private set;}
 public ScreenPicker(bool point){pointMode=point;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;ShowInTaskbar=false;Topmost=true;Background=Brushes.Black;Opacity=0.45;Cursor=Cursors.Cross;
  var screen=System.Windows.Forms.SystemInformation.VirtualScreen;
  SourceInitialized+=(s,e)=>{var source=System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(this).Handle);var transform=source.CompositionTarget.TransformFromDevice;var origin=transform.Transform(new Point(screen.X,screen.Y));var size=transform.Transform(new Point(screen.Width,screen.Height));Left=origin.X;Top=origin.Y;Width=size.X;Height=size.Y;};
  Content=canvas;canvas.Children.Add(outline);var help=new TextBlock{Text=point?"Click where the image should trigger a click. Esc cancels.":"Drag a rectangle to choose where to search. Esc cancels.",Foreground=Brushes.White,FontSize=20,FontWeight=FontWeights.Bold};Canvas.SetLeft(help,30);Canvas.SetTop(help,30);canvas.Children.Add(help);
  KeyDown+=(s,e)=>{if(e.Key==Key.Escape)DialogResult=false;};
  MouseLeftButtonDown+=(s,e)=>{start=e.GetPosition(canvas);dragging=true;CaptureMouse();};
  MouseMove+=(s,e)=>{if(!dragging||pointMode)return;var p=e.GetPosition(canvas);Canvas.SetLeft(outline,Math.Min(start.X,p.X));Canvas.SetTop(outline,Math.Min(start.Y,p.Y));outline.Width=Math.Abs(p.X-start.X);outline.Height=Math.Abs(p.Y-start.Y);};
  MouseLeftButtonUp+=(s,e)=>{if(!dragging)return;dragging=false;ReleaseMouseCapture();var a=PointToScreen(start);var b=PointToScreen(e.GetPosition(canvas));if(pointMode)Selection=new Drawing.Rectangle((int)b.X,(int)b.Y,1,1);else{Selection=new Drawing.Rectangle((int)Math.Min(a.X,b.X),(int)Math.Min(a.Y,b.Y),(int)Math.Abs(b.X-a.X),(int)Math.Abs(b.Y-a.Y));if(Selection.Width<2||Selection.Height<2)return;}DialogResult=true;};
 }
}}
