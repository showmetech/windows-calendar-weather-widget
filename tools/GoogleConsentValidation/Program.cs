using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CalendarWeather;
internal static class Program {
 [STAThread] static void Main(string[] args) {
  if(args.Length==0||!args.Contains("--self-test"))throw new InvalidOperationException("Perfil e marcador de isolamento obrigatórios.");
  var root=Path.GetFullPath(args[0]);
  if(!root.EndsWith(Path.DirectorySeparatorChar+"public-oauth-profile",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Perfil inesperado.");
  Store.Root=root;Directory.CreateDirectory(root);string report=Path.Combine(root,"consent-validation-status.json");bool running=true;string? error=null;
  void Save(){try{var accounts=Store.Accounts();int calendars=0,events=0,files=0;foreach(var path in Directory.GetFiles(root,"calendar-*.json")){var cache=JsonSerializer.Deserialize<CalendarSnapshot>(File.ReadAllText(path));if(cache!=null){files++;calendars+=cache.Calendars.Count;events+=cache.Events.Count;}}File.WriteAllText(report,JsonSerializer.Serialize(new{At=DateTimeOffset.Now,Running=running,ClientLoaded=GoogleClientStore.Load()!=null,AccountCount=accounts.Count,CalendarReadVerified=files>0,CalendarCount=calendars,EventCount=events,IsolatedProfile=true,StartupWritesDisabled=true,Error=error},new JsonSerializerOptions{WriteIndented=true}));}catch{File.WriteAllText(report,"{\"Running\":false,\"Error\":\"Falha ao verificar perfil isolado\"}");}}
  if(GoogleClientStore.Load()==null){error="Cliente indisponível neste contexto Windows";running=false;Save();return;}
  var app=new Application{ShutdownMode=ShutdownMode.OnMainWindowClose};app.Resources.MergedDictionaries.Add(new ResourceDictionary{Source=new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml")});
  var window=new Window{Title="Validação Google — perfil isolado",Width=550,Height=500,WindowStartupLocation=WindowStartupLocation.CenterScreen,FontFamily=new FontFamily("Segoe UI Variable"),FontSize=14};app.MainWindow=window;
  var panel=new StackPanel{Margin=new Thickness(24)};window.Content=panel;
  panel.Children.Add(new TextBlock{Text="Conexão Google do widget",FontSize=23,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,12)});
  panel.Children.Add(new TextBlock{Text="Teste isolado do cliente dedicado. A instalação atual, suas contas e a inicialização do Windows serão preservadas.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
  panel.Children.Add(new TextBlock{Text="Clique em Conectar Google e conclua o consentimento no navegador externo aberto pelo aplicativo. Use a conta proprietária do projeto. Não envie senhas ou códigos no chat.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
  var connect=new Button{Content="Conectar Google — somente leitura",Padding=new Thickness(12,8,12,8),HorizontalAlignment=HorizontalAlignment.Left};panel.Children.Add(connect);
  var status=new TextBlock{Text="Pronto para o primeiro consentimento.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,12)};panel.Children.Add(status);
  var show=new Button{Content="Abrir widget com os dados deste teste",IsEnabled=false,HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(12,6,12,6)};panel.Children.Add(show);
  var close=new Button{Content="Concluir e fechar teste",HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,12,0,0)};panel.Children.Add(close);using var ct=new CancellationTokenSource();
  var services=new Services();connect.Click+=async(_,_)=>{connect.IsEnabled=false;error=null;status.Text="Conclua a autorização no navegador. O prazo da conexão é de três minutos.";try{var account=await services.Connect("",ct.Token);var snapshot=await services.Calendar(account);Store.SaveAccounts(new(){account});Store.Save("calendar-"+account.Key+".json",snapshot);status.Text=$"Leitura real validada: {snapshot.Calendars.Count} calendários e {snapshot.Events.Count} eventos. Autorização salva somente neste perfil de teste.";show.IsEnabled=true;}catch(OperationCanceledException){status.Text="Autorização cancelada.";error="Cancelled";}catch(InvalidOperationException e){status.Text=e.Message;error=e.GetType().Name;}catch(Exception e){status.Text="A conexão não foi concluída. Confira o acesso e tente novamente.";error=e.GetType().Name;}finally{connect.IsEnabled=true;Save();}};
  show.Click+=(_,_)=>{var widget=new MainWindow{Title="Widget — perfil isolado de validação",ShowInTaskbar=true};widget.Show();};
  close.Click+=(_,_)=>app.Shutdown();window.Closing+=(_,_)=>ct.Cancel();app.Exit+=(_,_)=>{running=false;Save();};Save();app.Run(window);
 }
}
