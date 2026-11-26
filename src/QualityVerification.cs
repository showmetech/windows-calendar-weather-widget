using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
namespace CalendarWeather;
internal static class QualityVerification {
 internal static IEnumerable<T> Find<T>(DependencyObject root) where T:DependencyObject {
  if(root is T found)yield return found;
  for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var item in Find<T>(VisualTreeHelper.GetChild(root,i)))yield return item;
 }
 internal static async Task<Dictionary<string,bool>> Run(MainWindow window){
  var result=new Dictionary<string,bool>();
  foreach(var check in await GoogleFlowVerification.Run())result[check.Key]=check.Value;
  var connect=Find<Button>(window).First(x=>AutomationProperties.GetName(x)=="Conectar calendário");
  connect.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(400);
  var panel=Application.Current.Windows.Cast<Window>().Where(w=>w!=window).SelectMany(w=>Find<SettingsPanel>(w)).Single();
  result["connectOpensAccountsPage"]=panel.Pages[3].IsVisible;
  window.settings.LocationStatus="Localização bloqueada pelo Windows. Escolha uma cidade.";
  panel.RefreshState();panel.SelectPage(1);panel.UpdateLayout();
  result["locationFailureVisible"]=Find<TextBlock>(panel).Any(x=>x.Text==window.settings.LocationStatus&&x.IsVisible);
  window.CloseSettings();window.ShowSettings();await Task.Delay(700);
  result["settingsCanReopenDuringClosingAnimation"]=window.SettingsVisible;
  window.CloseSettings();await Task.Delay(400);
  int files=0,oauth=0;bool missing=false,emptyClientArgument=true;
  using(var googlePanel=new SettingsPanel(window,window.settings,Enumerable.Range(1,7).Select(i=>new Account{Email="Conta de exemplo "+i}).ToList(),new Services(new WeatherFixture()),()=>{files++;return null;},(file,ct)=>{oauth++;emptyClientArgument&=file=="";return missing?Task.FromException<Account>(new InvalidOperationException(GoogleClientStore.MissingConfigurationMessage)):Task.FromCanceled<Account>(new CancellationToken(true));})){
   var host=new Window{Content=new Border{Padding=new Thickness(20,64,20,12),Child=googlePanel},Width=360,Height=460,WindowStyle=WindowStyle.None,ShowInTaskbar=false};host.Show();googlePanel.SelectPage(3);host.UpdateLayout();
   var normal=Find<Button>(googlePanel).Single(b=>AutomationProperties.GetName(b)=="Conectar conta Google");normal.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(80);
   result["normalGoogleConnectNeverOpensFilePicker"]=files==0;
   result["normalGoogleConnectInvokesAuthorization"]=oauth==1&&emptyClientArgument;
   result["googleAuthorizationCancellationShown"]=Find<TextBlock>(googlePanel).Any(x=>x.Text=="Conexão cancelada. Você pode tentar novamente.");
   missing=true;normal.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(80);host.UpdateLayout();
   result["missingGoogleConfigurationDoesNotOpenPicker"]=files==0&&Find<TextBlock>(googlePanel).Any(x=>x.Text==GoogleClientStore.MissingConfigurationMessage&&x.IsVisible);
   result["googleErrorAndActionsFitWithManyAccounts"]=Find<FrameworkElement>(googlePanel).Where(x=>x.IsVisible&&(x is TextBlock||x is Button)).All(x=>{var p=x.TransformToAncestor(googlePanel).Transform(new Point(x.ActualWidth,x.ActualHeight));return p.X<=googlePanel.ActualWidth+1&&p.Y<=googlePanel.ActualHeight+1;});
   result["googleDeveloperSetupNotExposed"]=!Find<Button>(googlePanel).Any(b=>AutomationProperties.GetName(b).Contains("importar cliente OAuth"))&&files==0&&oauth==2;
   host.Close();
  }
  int pendingCalls=0;
  using(var pendingPanel=new SettingsPanel(window,window.settings,window.accounts,new Services(new WeatherFixture()),()=>throw new Exception("No file picker"),async(file,ct)=>{pendingCalls++;await Task.Delay(-1,ct);throw new Exception("Unreachable");})){
   var host=new Window{Content=pendingPanel,Width=360,Height=460,ShowInTaskbar=false};host.Show();pendingPanel.SelectPage(3);host.UpdateLayout();
   var normal=Find<Button>(pendingPanel).Single(b=>AutomationProperties.GetName(b)=="Conectar conta Google");normal.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));normal.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(40);
   var cancel=Find<Button>(pendingPanel).Single(b=>AutomationProperties.GetName(b)=="Cancelar conexão");
   result["googleRepeatedClickStartsOneAuthorization"]=pendingCalls==1&&!normal.IsEnabled&&cancel.IsVisible;
   cancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));await Task.Delay(40);
   result["googleCancelButtonRestoresActions"]=normal.IsEnabled&&!cancel.IsVisible&&Find<TextBlock>(pendingPanel).Any(x=>x.Text=="Conexão cancelada. Você pode tentar novamente.");
   var other=Find<Button>(pendingPanel).Single(b=>AutomationProperties.GetName(b)=="Adicionar assinatura ou arquivo");other.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));host.UpdateLayout();
   result["subscriptionSeparateFromGoogle"]=Find<ComboBox>(pendingPanel).Single(b=>AutomationProperties.GetName(b)=="Tipo de assinatura").IsVisible&&!normal.IsVisible;
   Find<Button>(pendingPanel).Single(b=>AutomationProperties.GetName(b)=="Voltar às contas").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));host.UpdateLayout();result["subscriptionBackReturnsToGoogle"]=normal.IsVisible;host.Close();
  }
  result["productGoogleClientMetadata"]=GoogleClientStore.ProductClient(new[]{new KeyValuePair<string,string?>("SMT.GoogleOAuth.DesktopClientId","synthetic.apps.googleusercontent.com"),new KeyValuePair<string,string?>("SMT.GoogleOAuth.DesktopClientSecret","synthetic-public-desktop-setting")})?.ClientId=="synthetic.apps.googleusercontent.com"&&GoogleClientStore.ProductClient(Array.Empty<KeyValuePair<string,string?>>())==null;
  var productMetadata=System.Reflection.CustomAttributeExtensions.GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>(System.Reflection.Assembly.GetEntryAssembly()!).Select(x=>new KeyValuePair<string,string?>(x.Key,x.Value)).ToList();
  result["productGoogleClientBuildContract"]=(GoogleClientStore.ProductClient(productMetadata)==null)==(GoogleClientStore.Load()==null);
  var handler=new DelayedWeather();var test=new MainWindow(new Services(handler));
  test.settings.AutomaticLocation=false;test.settings.LocationConfigured=true;test.settings.City="Cidade A";
  var first=test.Refresh(true);await handler.Started.Task;
  test.settings.City="Cidade B";test.settings.Latitude=10;var second=test.Refresh(true);handler.Release.TrySetResult(true);
  await Task.WhenAll(first,second);
  var saved=Store.Read<WeatherSnapshot?>("weather.json",null);
  result["refreshDoesNotLoseNewCityRequest"]=handler.Calls==2&&saved?.City=="Cidade B";
  var beforeFailure=System.IO.File.ReadAllText(System.IO.Path.Combine(Store.Root,"weather.json"));handler.Fail=true;await test.Refresh(true);
  var retained=Store.Read<WeatherSnapshot?>("weather.json",null);
  result["networkFailurePreservesWeatherCache"]=beforeFailure==System.IO.File.ReadAllText(System.IO.Path.Combine(Store.Root,"weather.json"));
  test.Show();await Task.Delay(150);test.UpdateLayout();
  result["networkFailureShown"]=Find<TextBlock>(test).Any(x=>x.Text.Contains("Clima sem conexão"));
  result["eventLinksUseHttpsOnly"]=Services.CanOpen("https://example.org/event")&&!Services.CanOpen("file:///c:/test")&&!Services.CanOpen("https://user:password@example.org/")&&!Services.CanOpen("");
  try{var empty=await new Services(new CalendarTestHandler{EmptyCalendars=true}).Calendar(new Account{RefreshToken="synthetic-refresh"});result["emptyGoogleCalendarList"]=empty.Calendars.Count==0&&empty.Events.Count==0;}catch{result["emptyGoogleCalendarList"]=false;}
  test.Hide();
  return result;
 }
 sealed class DelayedWeather:HttpMessageHandler {
  internal int Calls;internal bool Fail;internal readonly TaskCompletionSource<bool> Started=new(),Release=new();
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){
   if(++Calls==1){Started.TrySetResult(true);await Release.Task;}
   if(Fail)return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
   return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"current\":{\"temperature_2m\":22,\"apparent_temperature\":22,\"weather_code\":2,\"wind_speed_10m\":8},\"daily\":{\"time\":[\"2026-11-23\"],\"temperature_2m_min\":[18],\"temperature_2m_max\":[25],\"weather_code\":[2],\"precipitation_probability_max\":[10]}}",Encoding.UTF8,"application/json")};
  }
 }
}
