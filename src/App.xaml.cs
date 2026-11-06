using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Interop;
namespace CalendarWeather;
public partial class App:Application {
 static void Log(string step){try{System.IO.Directory.CreateDirectory(Store.Root);System.IO.File.AppendAllText(System.IO.Path.Combine(Store.Root,"startup.log"),DateTimeOffset.Now.ToString("o")+" "+step+"\n");}catch{}}
 string exitReason="unspecified";bool supervising;
 internal static void RequestExit(string reason){if(Current is App app)app.exitReason=reason;Current.Shutdown();}
 RegisteredWaitHandle? showWait,exitWait;
 DateTime lastHeartbeat=DateTime.MinValue;readonly DateTime started=DateTime.UtcNow;System.Windows.Threading.DispatcherTimer? maintenanceTimer;
 Mutex? mutex; EventWaitHandle? showEvent,exitEvent; NativeTray? tray; MainWindow? widget;
 protected override async void OnStartup(StartupEventArgs e){base.OnStartup(e);ShutdownMode=ShutdownMode.OnExplicitShutdown;SessionEnding+=(_,_)=>exitReason="windows-session-ending";DispatcherUnhandledException+=(_,err)=>{try{Store.Save("last-error.json",new{type=err.Exception.GetType().Name,message=err.Exception.Message,when=DateTimeOffset.Now});}catch{};};
 var dataIndex=Array.IndexOf(e.Args,"--data-root");if(dataIndex>=0&&dataIndex+1<e.Args.Length)Store.Root=Path.GetFullPath(e.Args[dataIndex+1]);
 if(e.Args.Contains("--recovery-test-child")){var index=Array.IndexOf(e.Args,"--recovery-test-child");Store.Root=Path.GetFullPath(e.Args[index+1]);Directory.CreateDirectory(Store.Root);var path=Path.Combine(Store.Root,"child-count.txt");int n=File.Exists(path)?int.Parse(File.ReadAllText(path))+1:1;File.WriteAllText(path,n.ToString());await System.Threading.Tasks.Task.Delay(100);Shutdown(n==1?71:0);return;}
 if(e.Args.Contains("--import-client")){var i=Array.IndexOf(e.Args,"--import-client");GoogleClientStore.Import(e.Args[i+1]);Shutdown();return;}
 if(e.Args.Contains("--self-test")){await Verification.Run(e.Args);Shutdown();return;}
 if(e.Args.Contains("--exit")){bool signaled=false;try{Maintenance.RequestExit();signaled=true;}catch{}try{using var exit=EventWaitHandle.OpenExisting("Local\\WindowsCalendarWeatherWidget.Exit");exit.Set();signaled=true;}catch{}Shutdown(signaled?0:1);return;}
 if(!e.Args.Contains("--managed")){supervising=true;exitReason="supervisor-finished";Shutdown(await RecoverySupervisor.Run(e.Args));return;}
 Log("begin "+Environment.ProcessId);mutex=new Mutex(true,"Local\\WindowsCalendarWeatherWidget.Singleton",out var first);
 Log("singleton="+first);if(!first){try{using var signal=EventWaitHandle.OpenExisting(e.Args.Contains("--exit")?"Local\\WindowsCalendarWeatherWidget.Exit":"Local\\WindowsCalendarWeatherWidget.Show");signal.Set();}catch{}Shutdown();return;}
 if(e.Args.Contains("--exit")){Shutdown();return;}
 showEvent=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\WindowsCalendarWeatherWidget.Show");
 exitEvent=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\WindowsCalendarWeatherWidget.Exit");
 showWait=ThreadPool.RegisterWaitForSingleObject(showEvent,(_,_)=>Dispatcher.BeginInvoke(()=>{widget?.Show();widget?.Activate();}),null,-1,false);
 exitWait=ThreadPool.RegisterWaitForSingleObject(exitEvent,(_,_)=>Dispatcher.BeginInvoke(()=>RequestExit("external-exit-signal")),null,-1,false);
 maintenanceTimer=new(){Interval=TimeSpan.FromSeconds(1)};maintenanceTimer.Tick+=(_,_)=>{try{if(DateTime.UtcNow-lastHeartbeat>TimeSpan.FromSeconds(10)){Directory.CreateDirectory(Store.Root);File.WriteAllText(RecoverySupervisor.HeartbeatPath,DateTime.UtcNow.ToString("O"));lastHeartbeat=DateTime.UtcNow;}}catch{}if(widget!=null&&tray!=null)LiveProbe.Poll(widget,widget.Show,widget.Hide,widget.ShowSettings,()=>widget.SettingsVisible,widget.CloseSettings,()=>tray.TryBounds(out _),widget.SettingsExpandedAbove,()=>new{LayoutVerified=widget.TwoEventsFit(),Accounts=widget.accounts.Count,Calendars=widget.snapshots.Values.Sum(s=>s.Calendars.Count),Events=widget.snapshots.Values.Sum(s=>s.Events.Count)});if(Maintenance.ExitRequested(started)){Log("maintenance exit");RequestExit("maintenance-request");}};maintenanceTimer.Start();
 Log("events ready");widget=new MainWindow();Log("window created");MainWindow=widget;
 widget.Show();widget.Activate();Log("shown="+widget.IsVisible);
 try{var icon=TrayIconFile();var handle=new WindowInteropHelper(widget).Handle;tray=new NativeTray(HwndSource.FromHwnd(handle)!,icon,()=>{if(widget.IsVisible)widget.Hide();else{widget.Show();widget.Activate();}},()=>{
 var menu=new ContextMenu();SuiteActivation.MenuOpen=true;menu.Closed+=(_,_)=>SuiteActivation.MenuOpen=false;void Add(string title,Action action){var item=new MenuItem{Header=title};item.Click+=(_,_)=>action();menu.Items.Add(item);}Add(widget.IsVisible?"Recolher widget":"Abrir widget",()=>{if(widget.IsVisible)widget.Hide();else{widget.Show();widget.Activate();}});Add("Atualizar",async()=>await widget.Refresh(true));Add("Configurações",widget.ShowSettings);menu.Items.Add(new Separator());Add("Sair",()=>RequestExit("tray-menu"));menu.IsOpen=true;});Log("tray ready");SystemEvents.UserPreferenceChanged+=TrayThemeChanged;}
 catch(Exception ex){widget.ShowInTaskbar=true;Log("tray unavailable "+ex.GetType().Name);}
 }
 protected override void OnExit(ExitEventArgs e){maintenanceTimer?.Stop();Log((supervising?"supervisor ":"")+"exit code="+e.ApplicationExitCode+" reason="+exitReason);SystemEvents.UserPreferenceChanged-=TrayThemeChanged;showWait?.Unregister(null);exitWait?.Unregister(null);tray?.Dispose();try{widget?.Persist();}catch(Exception ex){Log("save-on-exit failed "+ex.GetType().Name);}showEvent?.Dispose();exitEvent?.Dispose();mutex?.Dispose();base.OnExit(e);}
 internal static string TrayFileForTheme(bool lightTaskbar)=>"Calendar-Fluent.ico";
 static string TrayIconFile()=>Path.Combine(AppContext.BaseDirectory,"Assets","Calendar-Fluent.ico");
 void TrayThemeChanged(object sender,UserPreferenceChangedEventArgs e){Dispatcher.BeginInvoke(()=>tray?.UpdateIcon(TrayIconFile()));}
 public static System.Threading.Tasks.Task<bool> StartupEnabled()=>StartupService.EnabledAsync();
 public static System.Threading.Tasks.Task SetStartup(bool on)=>StartupService.Set(on);
}
