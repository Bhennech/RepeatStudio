using System;
using System.Linq;
using RepeatStudio.Settings;
using Drawing=System.Drawing;
namespace RepeatStudio {
public partial class MainWindow {
 void RestoreSettings(){if(Environment.GetCommandLineArgs().Length>1)return;var s=StudioSettings.Load();clickMinutes.Value=Math.Clamp(s.Minutes,0,60);clickSeconds.Value=Math.Clamp(s.Seconds,0,59);clickInterval.Value=Math.Clamp(s.Milliseconds,0,999);clickRepeats.Value=Math.Clamp(s.ClickRepeats,0,1000000);clickButton.SelectedIndex=Math.Clamp(s.MouseButton,0,4);repeats.Value=Math.Clamp(s.Repeats,0,100000);speed.Value=Math.Clamp(s.Speed,.25m,4m);imageRepeats.Value=Math.Clamp(s.ImageRepeats,0,1000000);imageFrequency.Value=Math.Clamp(s.ImageFrequency,0.001m,3600m);expanded=s.Sidebar;selectedPage=Math.Clamp(s.Page,0,2);
  if(s.Area?.Length==4&&s.Area[2]>1&&s.Area[3]>1){var area=new Drawing.Rectangle(s.Area[0],s.Area[1],s.Area[2],s.Area[3]);if(System.Windows.Forms.Screen.AllScreens.Any(screen=>screen.Bounds.IntersectsWith(area)))searchArea=area;}
  if(s.Position?.Length==2){var point=new Drawing.Point(s.Position[0],s.Position[1]);if(System.Windows.Forms.Screen.AllScreens.Any(screen=>screen.Bounds.Contains(point)))clickPoint=point;}
  limitArea.IsChecked=s.LimitArea&&searchArea.HasValue;fixedClick.IsChecked=s.FixedClick&&clickPoint.HasValue;UpdateSelections();ApplySidebar();
 }
 void SaveSettings(){if(Environment.GetCommandLineArgs().Length>1)return;new StudioSettings {Minutes=clickMinutes.Value,Seconds=clickSeconds.Value,Milliseconds=clickInterval.Value,ClickRepeats=clickRepeats.Value,MouseButton=clickButton.SelectedIndex,Repeats=repeats.Value,Speed=speed.Value,ImageRepeats=imageRepeats.Value,ImageFrequency=imageFrequency.Value,Sidebar=expanded,Page=selectedPage,Area=searchArea.HasValue?new[]{searchArea.Value.X,searchArea.Value.Y,searchArea.Value.Width,searchArea.Value.Height}:null,Position=clickPoint.HasValue?new[]{clickPoint.Value.X,clickPoint.Value.Y}:null,LimitArea=limitArea.IsChecked==true,FixedClick=fixedClick.IsChecked==true}.Save();}
}}
