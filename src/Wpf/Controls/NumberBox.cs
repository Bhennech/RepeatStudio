using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
namespace RepeatStudio.Controls {
public class NumberBox : TextBox {
 public decimal Minimum {get;set;} public decimal Maximum {get;set;}=1000000;public decimal Increment {get;set;}=1;public bool AllowFraction {get;set;}
 public decimal Value {get {decimal n;if(!decimal.TryParse(Text,NumberStyles.Number,CultureInfo.CurrentCulture,out n)||n<Minimum||n>Maximum||(!AllowFraction&&n!=decimal.Truncate(n)))throw new ArgumentException(System.Windows.Automation.AutomationProperties.GetName(this)+" must be "+(AllowFraction?"a number":"a whole number")+" between "+Minimum+" and "+Maximum+".");return n;}set {Text=value.ToString(CultureInfo.CurrentCulture);}}
 void Step(int direction){try{Value=Math.Clamp(Value+direction*Increment,Minimum,Maximum);}catch{Value=Minimum;}}
 public override void OnApplyTemplate(){base.OnApplyTemplate();if(GetTemplateChild("Increase") is System.Windows.Controls.Primitives.RepeatButton up)up.Click+=(s,e)=>Step(1);if(GetTemplateChild("Decrease") is System.Windows.Controls.Primitives.RepeatButton down)down.Click+=(s,e)=>Step(-1);}
 protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e){if(e.Key==System.Windows.Input.Key.Up||e.Key==System.Windows.Input.Key.Down){Step(e.Key==System.Windows.Input.Key.Up?1:-1);e.Handled=true;}base.OnPreviewKeyDown(e);}
}}
