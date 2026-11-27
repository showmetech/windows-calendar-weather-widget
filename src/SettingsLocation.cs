using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
namespace CalendarWeather;
public sealed partial class SettingsPanel {
 void BuildLocation(StackPanel page){
  var current=new StackPanel();var manual=new StackPanel{Visibility=Visibility.Collapsed};page.Children.Add(current);page.Children.Add(manual);
  current.Children.Add(MainWindow.Label("Localização da previsão",16,true));current.Children.Add(locationLabel);locationStatus.TextWrapping=TextWrapping.Wrap;locationStatus.Style=StatusStyle();current.Children.Add(locationStatus);
  var automatic=Switch("\uE81D","Localização automática",settings.AutomaticLocation);current.Children.Add(automatic);
  Button detect=null!;detect=ActionButton("\uE81D","Atualizar localização","Detectar localização",async()=>{automatic.IsChecked=true;settings.AutomaticLocation=true;await Detect();});
  async Task Detect(){detect.IsEnabled=false;try{locationStatus.Text="Detectando localização…";await main.DetectLocation(true);await main.Refresh(true);}finally{if(!disposed){detect.IsEnabled=true;RefreshState();}}}
  automatic.Click+=async(_,_)=>{settings.AutomaticLocation=automatic.IsChecked==true;main.Persist();if(settings.AutomaticLocation)await Detect();RefreshState();};current.Children.Add(detect);
  TextBox search=new(){MinHeight=32,VerticalContentAlignment=VerticalAlignment.Center};AutomationProperties.SetName(search,"Nome da cidade");
  current.Children.Add(ActionButton("\uE707","Escolher outra cidade","Escolher outra cidade",()=>{current.Visibility=Visibility.Collapsed;manual.Visibility=Visibility.Visible;search.Focus();}));
  manual.Children.Add(ActionButton("\uE72B","Voltar à localização","Voltar à localização",()=>{manual.Visibility=Visibility.Collapsed;current.Visibility=Visibility.Visible;automatic.Focus();}));
  manual.Children.Add(Hint("Pesquise a cidade e confirme o resultado desejado"));manual.Children.Add(Caption("\uE721","Nome da cidade"));manual.Children.Add(search);
  var results=new ComboBox{DisplayMemberPath="Name",Margin=new Thickness(0,8,0,4),Visibility=Visibility.Collapsed};AutomationProperties.SetName(results,"Cidades encontradas");
  var message=MainWindow.Label("",12);message.Style=StatusStyle();AutomationProperties.SetLiveSetting(message,AutomationLiveSetting.Polite);
  var find=ActionButton("\uE721","Pesquisar cidade","Pesquisar cidade",()=>{});manual.Children.Add(find);manual.Children.Add(results);
  var use=ActionButton("\uE73E","Usar cidade selecionada","Usar cidade selecionada",async()=>{if(results.SelectedItem is not City city)return;settings.AutomaticLocation=false;automatic.IsChecked=false;settings.City=city.Name;settings.Latitude=city.Lat;settings.Longitude=city.Lon;settings.LocationStatus="Cidade escolhida manualmente";main.Persist();await main.CityChanged();if(disposed)return;manual.Visibility=Visibility.Collapsed;current.Visibility=Visibility.Visible;RefreshState();automatic.Focus();});use.IsEnabled=false;use.ToolTip="Pesquise e selecione uma cidade";ToolTipService.SetShowOnDisabled(use,true);manual.Children.Add(use);manual.Children.Add(message);
  async Task Search(){if(!find.IsEnabled)return;if(string.IsNullOrWhiteSpace(search.Text)){message.Text="Informe o nome da cidade";search.Focus();return;}find.IsEnabled=false;message.Text="Pesquisando cidade…";try{var found=await service.Cities(search.Text.Trim());if(disposed)return;results.ItemsSource=found.Select(c=>new City(c.Name,c.Lat,c.Lon)).ToList();results.Visibility=found.Count>0?Visibility.Visible:Visibility.Collapsed;results.SelectedIndex=found.Count>0?0:-1;message.Text=found.Count==0?"Nenhuma cidade encontrada. Tente outro nome.":"";if(found.Count>0)results.Focus();}catch{if(!disposed)message.Text="Não foi possível pesquisar. Confira a conexão e tente novamente.";}finally{if(!disposed)find.IsEnabled=true;}}
  find.Click+=async(_,_)=>await Search();search.KeyDown+=async(_,e)=>{if(e.Key==Key.Return){e.Handled=true;await Search();}};results.SelectionChanged+=(_,_)=>{use.IsEnabled=results.SelectedItem is City;results.ToolTip=(results.SelectedItem as City)?.Name;};
 }
}
