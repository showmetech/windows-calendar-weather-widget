using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
namespace CalendarWeather;
internal static class Distribution {
 internal const string StartupTaskId="CalendarWeatherStartup";
 internal static bool IsPackaged {get {try{uint length=0;return GetCurrentPackageFullName(ref length,IntPtr.Zero)!=15700;}catch{return false;}}}
 internal static string ControlPath(string name){var directory=Path.Combine(Store.Root,"control");Directory.CreateDirectory(directory);return Path.Combine(directory,name);}
 internal static string LegacyPath(string name)=>Path.Combine(AppContext.BaseDirectory,name);
 internal static async Task<bool> StartupEnabled(){var task=await StartupTask.GetAsync(StartupTaskId);return task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;}
 internal static async Task SetStartup(bool enabled){var task=await StartupTask.GetAsync(StartupTaskId);if(!enabled){task.Disable();return;}var state=await task.RequestEnableAsync();if(state is not StartupTaskState.Enabled and not StartupTaskState.EnabledByPolicy)throw new InvalidOperationException("Ative o app em Configurações do Windows > Aplicativos > Inicialização. O Windows controla essa escolha.");}
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern int GetCurrentPackageFullName(ref uint length,IntPtr name);
}
