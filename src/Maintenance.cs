using System;
using System.Globalization;
using System.IO;
namespace CalendarWeather;
internal static class Maintenance {
 internal static string ExitPath=>Distribution.ControlPath(".widget-exit-request");
 internal static void RequestExit(string? path=null){path??=ExitPath;var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";File.WriteAllText(temp,DateTime.UtcNow.ToString("O",CultureInfo.InvariantCulture));File.Move(temp,path,true);}
 internal static bool ExitRequested(DateTime started,string? path=null){if(path==null)return ExitRequested(started,ExitPath)||(!Distribution.IsPackaged&&ExitRequested(started,Distribution.LegacyPath(".widget-exit-request")));try{var value=DateTime.Parse(File.ReadAllText(path),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);return value>=started&&value<=DateTime.UtcNow.AddSeconds(10);}catch{return false;}}
}
