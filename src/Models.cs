using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
namespace CalendarWeather;
public sealed class Settings {
 public int SchemaVersion {get;set;}
 public int LayoutVersion {get;set;}
 public bool StartWithWindows {get;set;}=true; public string StartupStatus {get;set;}="";
 public int WeatherRefreshMinutes {get;set;}=5; public int CalendarRefreshMinutes {get;set;}=5;
 public string DetectedLocationName {get;set;}=""; public string LocationNameKey {get;set;}="";
 public static readonly int[] RefreshOptions={1,5,10,15,30,60};
 public static int ValidInterval(int value)=>RefreshOptions.Contains(value)?value:5;
 public void Upgrade(){if(SchemaVersion<2){StartWithWindows=true;AutomaticLocation=true;WeatherRefreshMinutes=5;CalendarRefreshMinutes=5;SchemaVersion=2;}if(SchemaVersion<3){LocationConfigured=LocationUpdated.HasValue||LocationStatus=="Cidade escolhida manualmente";SchemaVersion=3;}WeatherRefreshMinutes=ValidInterval(WeatherRefreshMinutes);CalendarRefreshMinutes=ValidInterval(CalendarRefreshMinutes);}

 public double Left {get;set;}=double.NaN; public double Top {get;set;}=double.NaN;
 public double Width {get;set;}=360; public double Height {get;set;}=600;
 public bool Pinned {get;set;}=true;public string Theme {get;set;}="System"; public bool Topmost {get;set;}
 public bool AutomaticLocation {get;set;}=true; public string LocationStatus {get;set;}="Localização automática ainda não consultada"; public DateTimeOffset? LocationUpdated {get;set;}
 public bool LocationConfigured {get;set;}
 public string City {get;set;}="Localização pendente"; public double Latitude {get;set;}=-23.5505; public double Longitude {get;set;}=-46.6333;
 public List<string> HiddenCalendars {get;set;}=new();
}
public record CalendarInfo(string Id,string Name,string Color);
public record AgendaEvent(string Id,string CalendarId,string Calendar,string Color,string Title,DateTimeOffset Start,DateTimeOffset End,bool AllDay,string Url);
public record ForecastDay(string Date,double Min,double Max,int Code,int Rain);
public record WeatherSnapshot(string City,double Temperature,double Feels,int Code,double Wind,List<ForecastDay> Days,DateTimeOffset Updated);
public record CalendarSnapshot(List<AgendaEvent> Events,List<CalendarInfo> Calendars,DateTimeOffset Updated);
public sealed class Account {
 public string Provider {get;set;}="Google";public string FeedUrl {get;set;}="";public string SourceName {get;set;}="";public string Key {get;set;}=Guid.NewGuid().ToString("N"); public string Email {get;set;}="";
 public string ClientId {get;set;}=""; public string ClientSecret {get;set;}=""; public string RefreshToken {get;set;}="";
}
public static class Store {
 public static string Root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WindowsCalendarWeatherWidget");
 static readonly JsonSerializerOptions Json=new(){WriteIndented=true,NumberHandling=System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals};
 public static T Read<T>(string name,T fallback) {try{return JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(Root,name)),Json)??fallback;}catch{return fallback;}}
 public static void Save<T>(string name,T data) {Directory.CreateDirectory(Root);var p=Path.Combine(Root,name);File.WriteAllText(p+".tmp",JsonSerializer.Serialize(data,Json));File.Move(p+".tmp",p,true);}
 public static List<Account> Accounts(){try{return JsonSerializer.Deserialize<List<Account>>(Encoding.UTF8.GetString(Unprotect(File.ReadAllBytes(Path.Combine(Root,"accounts.dat")))))??new();}catch{return new();}}
 public static void SaveAccounts(List<Account> a){Directory.CreateDirectory(Root);var p=Path.Combine(Root,"accounts.dat");File.WriteAllBytes(p+".tmp",Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(a))));File.Move(p+".tmp",p,true);}
 public static byte[] Protect(byte[] b)=>Crypt(b,true); public static byte[] Unprotect(byte[] b)=>Crypt(b,false);
 [StructLayout(LayoutKind.Sequential)] struct Blob {public int Size;public IntPtr Data;}
 static byte[] Crypt(byte[] b,bool protect){var input=new Blob{Size=b.Length,Data=Marshal.AllocHGlobal(b.Length)};try{Marshal.Copy(b,0,input.Data,b.Length);Blob output;bool ok=protect?CryptProtectData(ref input,null,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,1,out output):CryptUnprotectData(ref input,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,1,out output);if(!ok)throw new System.ComponentModel.Win32Exception();try{var result=new byte[output.Size];Marshal.Copy(output.Data,result,0,result.Length);return result;}finally{LocalFree(output.Data);}}finally{Marshal.FreeHGlobal(input.Data);}}
 [DllImport("crypt32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool CryptProtectData(ref Blob input,string? description,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
 [DllImport("crypt32.dll",SetLastError=true)]static extern bool CryptUnprotectData(ref Blob input,IntPtr description,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
 [DllImport("kernel32.dll")]static extern IntPtr LocalFree(IntPtr h);
 public static string WeatherName(int c)=>c switch {0=>"Céu limpo",1=>"Predomínio de sol",2=>"Parcialmente nublado",3=>"Nublado",45 or 48=>"Nevoeiro",51 or 53 or 55=>"Garoa",56 or 57=>"Garoa congelante",61 or 63 or 65=>"Chuva",66 or 67=>"Chuva congelante",71 or 73 or 75 or 77=>"Neve",80 or 81 or 82=>"Pancadas de chuva",85 or 86=>"Pancadas de neve",95 or 96 or 99=>"Trovoadas",_=>"Condição indisponível"};
 public static string WeatherGlyph(int c)=>c switch {0 or 1=>"☀",2=>"⛅",3=>"☁",45 or 48=>"≋",>=95=>"ϟ",>=71 and <=77=>"❄",_=>"☂"};
 public static List<AgendaEvent> Upcoming(IEnumerable<AgendaEvent> events,DateTimeOffset now)=>events.Where(e=>e.End>now).OrderBy(e=>e.Start).ThenBy(e=>e.Title).ToList();
}
