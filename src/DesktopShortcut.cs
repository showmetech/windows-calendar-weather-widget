using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel;
namespace CalendarWeather;
internal static class DesktopShortcut {
 internal const string CurrentName="SMT Calendar Widget.lnk", LegacyName="Windows Calendar e Weather Widget.lnk";
 internal static bool Owned(string target,string arguments,string family){
  try{return string.Equals(Path.GetFullPath(target),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"explorer.exe"),StringComparison.OrdinalIgnoreCase)&&string.Equals(arguments.Trim(),"shell:AppsFolder\\"+family+"!CalendarWeather",StringComparison.OrdinalIgnoreCase);}catch{return false;}
 }
 static void Release(object value){if(Marshal.IsComObject(value))Marshal.FinalReleaseComObject(value);}
 internal static string Repair(string desktop,string profile,string sourceIcon,string family){
  string current=Path.Combine(desktop,CurrentName),legacy=Path.Combine(desktop,LegacyName);
  string path=File.Exists(current)?current:legacy;
  if(!File.Exists(path))return "not-present";
  byte[] original=File.ReadAllBytes(path);
  var type=Type.GetTypeFromProgID("WScript.Shell")??throw new InvalidOperationException("Windows shortcuts unavailable.");
  object shellObject=Activator.CreateInstance(type)!;dynamic shell=shellObject;
  try{
   object linkObject=shell.CreateShortcut(path);dynamic link=linkObject;
   try{
    if(!Owned((string)link.TargetPath,(string)link.Arguments,family))return "not-owned";
    if(!File.ReadAllBytes(path).SequenceEqual(original))return "changed-by-user";
    string icon=Path.Combine(profile,"Assets","Calendar-Fluent.ico");
    byte[] desired=File.ReadAllBytes(sourceIcon);
    bool iconChanged=!File.Exists(icon)||!File.ReadAllBytes(icon).SequenceEqual(desired);
    bool rename=path==legacy, repoint=!string.Equals((string)link.IconLocation,icon+",0",StringComparison.OrdinalIgnoreCase);
    if(!iconChanged&&!rename&&!repoint)return "unchanged";
    if(iconChanged){Directory.CreateDirectory(Path.GetDirectoryName(icon)!);string temp=icon+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(temp,desired);File.Move(temp,icon,true);}finally{if(File.Exists(temp))File.Delete(temp);}}
    if(!File.ReadAllBytes(path).SequenceEqual(original))return "changed-by-user";
    if(rename||repoint){
     string backups=Path.Combine(profile,"shortcut-backups");Directory.CreateDirectory(backups);
     File.Copy(path,Path.Combine(backups,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")+".lnk"));
     if(repoint){link.IconLocation=icon+",0";link.Save();}
     if(rename)File.Move(path,current);
    }
    SHChangeNotify(0x00002000,0x0005,current,IntPtr.Zero);
    return "updated";
   }finally{Release(linkObject);}
  }finally{Release(shellObject);}
 }
 internal static void StartMaintenance(Action<string> log){
  if(!Distribution.IsPackaged)return;
  var worker=new Thread(()=>{try{log("desktop shortcut "+Repair(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),Store.Root,Path.Combine(AppContext.BaseDirectory,"Assets","Calendar-Fluent.ico"),Package.Current.Id.FamilyName));}catch(Exception error){log("desktop shortcut maintenance unavailable "+error.GetType().Name);}}){IsBackground=true,Name="Calendar shortcut maintenance",Priority=ThreadPriority.BelowNormal};
  worker.SetApartmentState(ApartmentState.STA);worker.Start();
 }
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern void SHChangeNotify(uint action,uint flags,string path,IntPtr second);
}
