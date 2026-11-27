using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
namespace CalendarWeather;
internal static class DesktopShortcutVerification {
 internal static Dictionary<string,bool> Run(string folder){
  const string family="SMT.Isolated.ShortcutTest_fixture";
  string fixture=Path.Combine(folder,Guid.NewGuid().ToString("N"));string desktop=Path.Combine(fixture,"Desktop"),profile=Path.Combine(fixture,"Profile"),icon=Path.Combine(AppContext.BaseDirectory,"Assets","Calendar-Fluent.ico");
  Directory.CreateDirectory(desktop);var results=new Dictionary<string,bool>();
  var type=Type.GetTypeFromProgID("WScript.Shell")??throw new InvalidOperationException("Shortcut test COM unavailable");object shellObject=Activator.CreateInstance(type)!;dynamic shell=shellObject;
  void Create(string path,string target,string args){object value=shell.CreateShortcut(path);dynamic link=value;try{link.TargetPath=target;link.Arguments=args;link.IconLocation="missing-old-version.exe,0";link.Save();}finally{Marshal.FinalReleaseComObject(value);}}
  string legacy=Path.Combine(desktop,DesktopShortcut.LegacyName),current=Path.Combine(desktop,DesktopShortcut.CurrentName),explorer=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"explorer.exe"),args="shell:AppsFolder\\"+family+"!CalendarWeather";
  try{
   results["absentShortcutNotRecreated"]=DesktopShortcut.Repair(desktop,profile,icon,family)=="not-present"&&!Directory.Exists(profile);
   Create(legacy,explorer,args);byte[] old=File.ReadAllBytes(legacy);
   results["ownedLegacyShortcutMigrated"]=DesktopShortcut.Repair(desktop,profile,icon,family)=="updated"&&File.Exists(current)&&!File.Exists(legacy);
   string stableIcon=Path.Combine(profile,"Assets","Calendar-Fluent.ico");
   results["stableDesktopIconMatchesTrayAsset"]=File.ReadAllBytes(stableIcon).SequenceEqual(File.ReadAllBytes(icon));
   results["shortcutBackupPreservesOriginal"]=Directory.GetFiles(Path.Combine(profile,"shortcut-backups"),"*.lnk").Any(p=>File.ReadAllBytes(p).SequenceEqual(old));
   byte[] before=File.ReadAllBytes(current);results["repeatMaintenanceIsIdempotent"]=DesktopShortcut.Repair(desktop,profile,icon,family)=="unchanged"&&File.ReadAllBytes(current).SequenceEqual(before);
   File.WriteAllBytes(stableIcon,new byte[]{1,2,3});results["staleIconRepairedAfterUpdate"]=DesktopShortcut.Repair(desktop,profile,icon,family)=="updated"&&File.ReadAllBytes(stableIcon).SequenceEqual(File.ReadAllBytes(icon));
   Create(current,explorer,"shell:AppsFolder\\Other.App_fixture!App");byte[] foreign=File.ReadAllBytes(current);
   results["foreignShortcutPreserved"]=DesktopShortcut.Repair(desktop,profile,icon,family)=="not-owned"&&File.ReadAllBytes(current).SequenceEqual(foreign);
   results["unexpectedExecutableRejected"]=!DesktopShortcut.Owned(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"notepad.exe"),args,family);
   results["extraArgumentsRejected"]=!DesktopShortcut.Owned(explorer,args+" --other",family);
  }finally{Marshal.FinalReleaseComObject(shellObject);}
  return results;
 }
}
