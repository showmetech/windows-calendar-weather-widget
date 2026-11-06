using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
internal static class RecoverySupervisor {
 internal static string HeartbeatPath=>Path.Combine(Store.Root,"ui-heartbeat.txt");
 internal static bool ShouldRecover(int exitCode,int recentRestarts)=>exitCode!=0&&recentRestarts<3;
 internal static async Task<int> Monitor(string executable,string[] arguments,Action<string> log,bool checkHang=true,int retryDelayMs=2000){
  int restarts=0;var window=DateTime.UtcNow;
  while(true){
   var start=new ProcessStartInfo(executable){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=AppContext.BaseDirectory};foreach(var arg in arguments)start.ArgumentList.Add(arg);
   using var child=Process.Start(start)??throw new InvalidOperationException("Não foi possível abrir o widget.");var began=DateTime.UtcNow;log("child started pid="+child.Id);bool hung=false;var done=child.WaitForExitAsync();
   while(!child.HasExited){
    if(await Task.WhenAny(done,Task.Delay(5000))==done)break;
    if(checkHang&&DateTime.UtcNow-began>TimeSpan.FromMinutes(3)){
     DateTime beat=File.Exists(HeartbeatPath)?File.GetLastWriteTimeUtc(HeartbeatPath):began;
     if(DateTime.UtcNow-beat>TimeSpan.FromMinutes(2)&&!child.Responding){log("unresponsive child; restarting");hung=true;child.Kill();await child.WaitForExitAsync();break;}
    }
   }
   int code=hung?-1:child.ExitCode;log("child exit="+code);
   if(DateTime.UtcNow-window>TimeSpan.FromMinutes(10)){restarts=0;window=DateTime.UtcNow;}
   if(!ShouldRecover(code,restarts))return code;
   restarts++;log("recovery attempt="+restarts);await Task.Delay(retryDelayMs*restarts);
  }
 }
 internal static async Task<int> Run(string[] arguments){
  using var mutex=new Mutex(true,"Local\\WindowsCalendarWeatherWidget.Recovery",out var first);
  if(!first){if(!arguments.Contains("--startup")){try{using var show=EventWaitHandle.OpenExisting("Local\\WindowsCalendarWeatherWidget.Show");show.Set();}catch{}}return 0;}
  Directory.CreateDirectory(Store.Root);
  void Log(string message){try{File.AppendAllText(Path.Combine(Store.Root,"recovery.log"),DateTimeOffset.Now.ToString("O")+" "+message+Environment.NewLine);}catch{}}
  try{return await Monitor(Environment.ProcessPath!,arguments.Concat(new[]{"--managed"}).ToArray(),Log);}catch(Exception ex){Log("supervisor error="+ex.GetType().Name);return 1;}
 }
 internal static async Task<bool> Verify(string folder){
  Directory.CreateDirectory(folder);var count=Path.Combine(folder,"child-count.txt");if(File.Exists(count))File.Delete(count);int attempts=0;
  int result=await Monitor(Environment.ProcessPath!,new[]{"--recovery-test-child",folder},message=>{if(message.StartsWith("child started"))attempts++;},false,30);
  return result==0&&attempts==2&&File.ReadAllText(count)=="2"&&!ShouldRecover(0,0)&&!ShouldRecover(9,3);
 }
}
