using System;
using System.IO;
using System.Text.Json;
namespace RepeatStudio.Settings {
public class StudioSettings {
 public decimal Minutes {get;set;} public decimal Seconds {get;set;}=1;public decimal Milliseconds {get;set;}public decimal ClickRepeats {get;set;}=1;public int MouseButton {get;set;}public decimal Repeats {get;set;}=1;public decimal Speed {get;set;}=1;public bool Sidebar {get;set;}=true;public int Page {get;set;}
 public int[] Area {get;set;}public int[] Position {get;set;}public bool LimitArea {get;set;}public bool FixedClick {get;set;}
 static string Folder=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"RepeatStudio");
 public static StudioSettings Load(){try{return JsonSerializer.Deserialize<StudioSettings>(File.ReadAllText(Path.Combine(Folder,"settings.json")))??new StudioSettings();}catch{return new StudioSettings();}}
 public void Save(){Directory.CreateDirectory(Folder);var path=Path.Combine(Folder,"settings.json");File.WriteAllText(path+".tmp",JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));File.Move(path+".tmp",path,true);}
}
}
