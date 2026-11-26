using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
namespace CalendarWeather;
public sealed partial class SettingsPanel {
 CancellationTokenSource? connectionCancellation;
 static TextBlock Hint(string text)=>new(){Text=text,FontSize=12,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,4,0,8)};
 static void Primary(Button button){button.Background=SystemColors.AccentColorBrush;var c=SystemColors.AccentColor;button.Foreground=c.R*.2126+c.G*.7152+c.B*.0722>145?Brushes.Black:Brushes.White;}
 void BuildConnections(StackPanel page,Func<string,CancellationToken,Task<Account>> authorize){
  var home=new StackPanel();var subscription=new StackPanel{Visibility=Visibility.Collapsed};page.Children.Add(home);page.Children.Add(subscription);
  home.Children.Add(MainWindow.Label("Seus calendários",16,true));home.Children.Add(Hint("Conecte sua conta para mostrar os próximos eventos. Acesso somente de leitura."));home.Children.Add(accountPanel);
  var google=ActionButton("\uE8FA","Conectar conta Google","Conectar conta Google",()=>{});google.Margin=new Thickness(0,8,0,4);google.MinHeight=36;Primary(google);home.Children.Add(google);
  var other=ActionButton("\uE710","Adicionar assinatura ou arquivo","Adicionar assinatura ou arquivo",()=>{home.Visibility=Visibility.Collapsed;subscription.Visibility=Visibility.Visible;accountStatus.Text="";});home.Children.Add(other);
  var cancel=ActionButton("\uE711","Cancelar conexão","Cancelar conexão",()=>connectionCancellation?.Cancel());cancel.Visibility=Visibility.Collapsed;home.Children.Add(cancel);
  google.Click+=async(_,_)=>{
   if(connectionCancellation!=null)return;
   using var attempt=CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token);connectionCancellation=attempt;
   google.IsEnabled=false;other.Visibility=Visibility.Collapsed;cancel.Visibility=Visibility.Visible;accountStatus.Text="No navegador, escolha sua conta Google e autorize a leitura da agenda.";
   try{
    var account=await authorize("",attempt.Token);attempt.Token.ThrowIfCancellationRequested();if(disposed)return;
    var old=accounts.FirstOrDefault(a=>a.Provider=="Google"&&string.Equals(a.Email,account.Email,StringComparison.OrdinalIgnoreCase));
    if(old!=null){account.Key=old.Key;accounts.Remove(old);}accounts.Add(account);main.AccountsChanged();
    Window.GetWindow(this)?.Activate();accountStatus.Text="Conta conectada. Atualizando eventos…";RefreshAccounts();await main.Refresh(true);if(disposed)return;
    accountStatus.Text="Conta conectada: "+account.Email+". Escolha os calendários em Agendas.";
   }catch(OperationCanceledException){if(!disposed)accountStatus.Text="Conexão cancelada. Você pode tentar novamente.";}
   catch(TimeoutException){if(!disposed)accountStatus.Text="A autorização expirou. Clique em Conectar conta Google para tentar novamente.";}
   catch(InvalidOperationException ex){if(!disposed)accountStatus.Text=ex.Message;}
   catch{if(!disposed)accountStatus.Text="Não foi possível conectar ao Google. Confira a internet e tente novamente.";}
   finally{connectionCancellation=null;if(!disposed){google.IsEnabled=true;other.Visibility=Visibility.Visible;cancel.Visibility=Visibility.Collapsed;RefreshCalendars();}}
  };
  var back=ActionButton("\uE72B","Voltar às contas","Voltar às contas",()=>{subscription.Visibility=Visibility.Collapsed;home.Visibility=Visibility.Visible;accountStatus.Text="";});subscription.Children.Add(back);
  subscription.Children.Add(Hint("Use um link de assinatura do Outlook, iCloud ou outro serviço, ou um arquivo .ics."));
  var provider=new ComboBox{ItemsSource=new[]{"Link de assinatura","Arquivo iCalendar (.ics)"},SelectedIndex=0,Margin=new Thickness(0,0,0,8)};AutomationProperties.SetName(provider,"Tipo de assinatura");subscription.Children.Add(provider);
  var name=new TextBox{MinHeight=32,VerticalContentAlignment=VerticalAlignment.Center};AutomationProperties.SetName(name,"Nome do calendário (opcional)");subscription.Children.Add(Caption("\uE8D2","Nome (opcional)"));subscription.Children.Add(name);
  var linkLabel=Caption("\uE71B","Link HTTPS ou webcal");var link=new TextBox{MinHeight=32,Margin=new Thickness(0,4,0,8),VerticalContentAlignment=VerticalAlignment.Center};AutomationProperties.SetName(link,"Link de assinatura do calendário");subscription.Children.Add(linkLabel);subscription.Children.Add(link);
  var add=ActionButton("\uE710","Adicionar calendário","Adicionar calendário",()=>{});Primary(add);subscription.Children.Add(add);
  provider.SelectionChanged+=(_,_)=>{linkLabel.Visibility=link.Visibility=provider.SelectedIndex==0?Visibility.Visible:Visibility.Collapsed;accountStatus.Text="";};
  add.Click+=async(_,_)=>{if(!add.IsEnabled)return;add.IsEnabled=false;back.IsEnabled=false;try{
   string address=link.Text.Trim();bool local=provider.SelectedIndex==1;
   if(local){var dialog=new OpenFileDialog{Title="Abrir calendário iCalendar",Filter="Calendário (*.ics)|*.ics"};if(dialog.ShowDialog(Window.GetWindow(this)??main)!=true){accountStatus.Text="Seleção cancelada.";return;}address=dialog.FileName;}else address=Services.SubscriptionUri(address).AbsoluteUri;
   if(accounts.Any(a=>a.Provider!="Google"&&a.FeedUrl==address))throw new InvalidOperationException("Esse calendário já está conectado.");
   var label=string.IsNullOrWhiteSpace(name.Text)?"Calendário iCalendar":name.Text.Trim();var account=new Account{Provider=local?"LocalIcs":"Ics",FeedUrl=address,Email=label,SourceName=label};
   accountStatus.Text="Verificando calendário…";var snapshot=await service.Subscription(account,cancellation.Token);if(disposed)return;
   accounts.Add(account);main.snapshots[account.Key]=snapshot;Store.Save("calendar-"+account.Key+".json",snapshot);main.AccountsChanged();RefreshAccounts();RefreshCalendars();link.Clear();name.Clear();subscription.Visibility=Visibility.Collapsed;home.Visibility=Visibility.Visible;accountStatus.Text="Calendário adicionado.";
  }catch(OperationCanceledException){if(!disposed)accountStatus.Text="Conexão cancelada.";}catch(InvalidOperationException ex){if(!disposed)accountStatus.Text=ex.Message;}catch{if(!disposed)accountStatus.Text="Não foi possível ler o calendário. Confira o link e o acesso.";}finally{if(!disposed){add.IsEnabled=true;back.IsEnabled=true;}}};
  accountStatus.Margin=new Thickness(0,8,0,0);accountStatus.TextWrapping=TextWrapping.Wrap;accountStatus.Style=StatusStyle();AutomationProperties.SetLiveSetting(accountStatus,AutomationLiveSetting.Polite);page.Children.Add(accountStatus);
 }
}
