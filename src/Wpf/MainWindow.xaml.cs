using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RepeatStudio.Automation;
using RepeatStudio.Controls;
using Drawing = System.Drawing;
namespace RepeatStudio {
public partial class MainWindow : Window {
 readonly AutomationEngine engine=new AutomationEngine();
 Drawing.Bitmap targetImage; Drawing.Rectangle? searchArea; Drawing.Point? clickPoint;
 int selectedPage; bool expanded=true,captureReady; IntPtr handle; HwndSource source;
 readonly DispatcherTimer countTimer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(300) };
 public MainWindow(){InitializeComponent();engine.Status+=text=>status.Text=text;engine.Changed+=RefreshState;
  SourceInitialized+=InitializeNative;Closed+=Cleanup;
  PreviewKeyDown+=(s,e)=>{if(e.Key==Key.V&&Keyboard.Modifiers==ModifierKeys.Control&&selectedPage==2){PasteTarget();e.Handled=true;}};
  countTimer.Tick+=(s,e)=>{if(engine.Recording)count.Text=engine.Count+" input events recorded";};countTimer.Start();
  StateChanged+=(s,e)=>maximize.Content=WindowState==WindowState.Maximized?"❐":"□";
  RestoreSettings();SelectPage(selectedPage);RefreshState();
  Loaded+=OnLoaded;
 }
 void InitializeNative(object sender,EventArgs e){handle=new WindowInteropHelper(this).Handle;source=HwndSource.FromHwnd(handle);source.AddHook(WindowMessage);
  try{engine.Initialize();captureReady=true;}catch(Exception ex){status.Text=ex.Message;record.IsEnabled=false;}
  for(int i=0;i<3;i++)if(!RegisterHotKey(handle,i+1,0x4000,(uint)(0x77+i)))status.Text="Shortcut F"+(8+i)+" is in use. Close the other shortcut app and restart.";
  RefreshState();
 }
 IntPtr WindowMessage(IntPtr hwnd,int msg,IntPtr w,IntPtr l,ref bool handled){if(msg==0x312){handled=true;switch(w.ToInt32()){case 1:engine.ToggleRecord();break;case 2:_=StartCurrent();break;case 3:engine.Stop();break;}}return IntPtr.Zero;}
 void RefreshState(){bool idle=!engine.Playing&&!engine.Recording;
  foreach(UIElement c in new UIElement[]{clickMinutes,clickSeconds,clickInterval,clickRepeats,clickButton,startClicks,repeats,speed,imageRepeats,imageFrequency,pasteImage,chooseArea,chooseClick,limitArea,fixedClick,clear})c.IsEnabled=idle;
  play.IsEnabled=idle&&engine.Count>0;findImage.IsEnabled=idle&&targetImage!=null;stopClicks.IsEnabled=engine.Playing;record.IsEnabled=!engine.Playing&&captureReady;
  record.Content=engine.Recording?"Finish · F8":"Record · F8";count.Text=engine.Count+" input events recorded";
 }
 async Task StartCurrent(){if(engine.Playing||engine.Recording)return;
  try{try{SaveSettings();}catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}if(selectedPage==0){int interval=checked((int)(clickMinutes.Value*60000+clickSeconds.Value*1000+clickInterval.Value));if(interval<1)throw new ArgumentException("Set an interval of at least 1 millisecond.");await engine.Simple(interval,(int)clickRepeats.Value,clickButton.SelectedIndex);}
   else if(selectedPage==1)await engine.Playback((int)repeats.Value,(double)speed.Value);
   else {if(targetImage==null){status.Text="Copy an image first, then choose Paste image.";return;}if(fixedClick.IsChecked==true&&!clickPoint.HasValue)throw new ArgumentException("Choose a fixed click position first.");if(limitArea.IsChecked==true&&!searchArea.HasValue)throw new ArgumentException("Choose a search area first.");if(fixedClick.IsChecked==true&&!System.Windows.Forms.Screen.AllScreens.Any(s=>s.Bounds.Contains(clickPoint.Value)))throw new ArgumentException("Click position is off screen. Choose it again.");using(var template=new Drawing.Bitmap(targetImage)){await engine.Search(template,limitArea.IsChecked==true?searchArea:null,fixedClick.IsChecked==true?clickPoint:null,()=>WindowState=WindowState.Minimized,(int)imageRepeats.Value,checked((int)(imageFrequency.Value*1000)));}}
  }catch(Exception ex){status.Text=ex.Message;}
 }
 async void Start(object sender,RoutedEventArgs e){await StartCurrent();}
 void Stop(object sender,RoutedEventArgs e){engine.Stop();}
 void Record(object sender,RoutedEventArgs e){engine.ToggleRecord();}
 void Clear(object sender,RoutedEventArgs e){engine.Clear();}
 void Paste(object sender,RoutedEventArgs e){PasteTarget();}
 void PasteTarget(){if(engine.Playing||engine.Recording)return;try{var image=System.Windows.Clipboard.GetImage();if(image==null){status.Text="Copy an image first, then choose Paste image.";return;}var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using(var stream=new MemoryStream()){encoder.Save(stream);stream.Position=0;using(var sourceImage=new Drawing.Bitmap(stream)){var next=new Drawing.Bitmap(sourceImage.Width,sourceImage.Height,Drawing.Imaging.PixelFormat.Format24bppRgb);using(var g=Drawing.Graphics.FromImage(next))g.DrawImageUnscaled(sourceImage,0,0);targetImage?.Dispose();targetImage=next;}}preview.Source=image;status.Text="Image ready. Press Search & click or F9.";RefreshState();}catch(Exception ex){status.Text="Could not paste image: "+ex.Message;}}
 void ChooseArea(object sender,RoutedEventArgs e){SelectScreen(false);}
 void ChoosePosition(object sender,RoutedEventArgs e){SelectScreen(true);}
 void AreaChecked(object sender,RoutedEventArgs e){if(IsLoaded&&!searchArea.HasValue)SelectScreen(false);}
 void PositionChecked(object sender,RoutedEventArgs e){if(IsLoaded&&!clickPoint.HasValue)SelectScreen(true);}
 void SelectScreen(bool point){if(engine.Recording||engine.Playing)return;Hide();try{var picker=new ScreenPicker(point);if(picker.ShowDialog()==true){if(point){clickPoint=picker.Selection.Location;fixedClick.IsChecked=true;}else{searchArea=picker.Selection;limitArea.IsChecked=true;}}else{if(point&&!clickPoint.HasValue)fixedClick.IsChecked=false;if(!point&&!searchArea.HasValue)limitArea.IsChecked=false;}UpdateSelections();}finally{Show();Activate();}}
 void UpdateSelections(){areaInfo.Text=searchArea.HasValue?"Area: X "+searchArea.Value.X+", Y "+searchArea.Value.Y+" — "+searchArea.Value.Width+" × "+searchArea.Value.Height:"No area selected";clickInfo.Text=clickPoint.HasValue?"Click position: X "+clickPoint.Value.X+", Y "+clickPoint.Value.Y:"No fixed position selected";}
 void Navigate(object sender,RoutedEventArgs e){SelectPage(int.Parse((string)((Button)sender).Tag));}
 void SelectPage(int page){selectedPage=page;simplePage.Visibility=page==0?Visibility.Visible:Visibility.Collapsed;recordedPage.Visibility=page==1?Visibility.Visible:Visibility.Collapsed;imagePage.Visibility=page==2?Visibility.Visible:Visibility.Collapsed;var buttons=new[]{simpleTab,recordedTab,imageTab};for(int i=0;i<3;i++){buttons[i].Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(i==page?"#6C4191":"#F2DDED"));buttons[i].Foreground=i==page?Brushes.White:new SolidColorBrush((Color)ColorConverter.ConvertFromString("#492F5E"));}pageScroll.ScrollToTop();}
 void ToggleTabs(object sender,RoutedEventArgs e){expanded=!expanded;ApplySidebar();}
 void ApplySidebar(){navigation.Visibility=expanded?Visibility.Visible:Visibility.Collapsed;sidebarColumn.Width=new GridLength(expanded?200:0);toggleTabs.Content=expanded?"◀ Hide tabs":"▶ Show tabs";if(expanded){Grid.SetColumn(pageScroll,1);Grid.SetColumnSpan(pageScroll,1);pageScroll.Margin=new Thickness(0);}else{Grid.SetColumn(pageScroll,0);Grid.SetColumnSpan(pageScroll,2);pageScroll.Margin=new Thickness(0,48,0,0);}}
 void Minimize(object sender,RoutedEventArgs e){WindowState=WindowState.Minimized;}
 void Maximize(object sender,RoutedEventArgs e){WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;}
 void CloseWindow(object sender,RoutedEventArgs e){Close();}
 void Cleanup(object sender,EventArgs e){countTimer.Stop();engine.Dispose();targetImage?.Dispose();for(int i=1;i<=3;i++)UnregisterHotKey(handle,i);source?.RemoveHook(WindowMessage);try{SaveSettings();}catch{} }
 [DllImport("user32.dll")]static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
 [DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr window,int id);
}
}
