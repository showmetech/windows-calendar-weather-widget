using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Markup;
namespace CalendarWeather;
internal static class FluentDesign {
 internal const double Radius=8, Inset=20, Caption=12, Body=14, Subtitle=20;
 internal static double Luminance(Color c){static double Linear(byte b){double v=b/255.0;return v<=.04045?v/12.92:Math.Pow((v+.055)/1.055,2.4);}return .2126*Linear(c.R)+.7152*Linear(c.G)+.0722*Linear(c.B);}
 internal static double Contrast(Color a,Color b){double x=Luminance(a),y=Luminance(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);}
 internal static Brush OnAccent(Color c)=>Contrast(Colors.Black,c)>=Contrast(Colors.White,c)?Brushes.Black:Brushes.White;
 internal static Brush Surface(bool light)=>SystemParameters.HighContrast?SystemColors.WindowBrush:new SolidColorBrush(light?Color.FromRgb(243,243,243):Color.FromRgb(32,32,32));
 internal static Brush Card(bool light)=>SystemParameters.HighContrast?SystemColors.WindowBrush:new SolidColorBrush(light?Colors.White:Color.FromRgb(44,44,44));
 internal static Brush Secondary(bool light)=>SystemParameters.HighContrast?SystemColors.WindowTextBrush:new SolidColorBrush(light?Color.FromRgb(88,88,88):Color.FromRgb(193,193,193));
 internal static bool Motion(Settings settings)=>SystemParameters.ClientAreaAnimation&&!settings.ReduceMotion&&!SystemParameters.HighContrast;
 internal static void ThemeResources(){Application.Current.Resources["SMT.AccentBrush"]=SystemParameters.HighContrast?SystemColors.HighlightBrush:SystemColors.AccentColorBrush;Application.Current.Resources["SMT.OnAccentBrush"]=SystemParameters.HighContrast?SystemColors.HighlightTextBrush:OnAccent(SystemColors.AccentColor);}
 internal static Style SwitchStyle(){var style=new Style(typeof(CheckBox));style.Setters.Add(new Setter(Control.TemplateProperty,(ControlTemplate)XamlReader.Parse("""
<ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="CheckBox">
 <Border x:Name="Focus" BorderBrush="Transparent" BorderThickness="2" CornerRadius="4" Padding="2">
  <Grid><Grid.ColumnDefinitions><ColumnDefinition Width="40"/><ColumnDefinition Width="*"/></Grid.ColumnDefinitions>
   <Grid VerticalAlignment="Center" Height="20" Width="38" HorizontalAlignment="Left">
    <Border x:Name="Track" BorderBrush="{TemplateBinding Foreground}" BorderThickness="1" CornerRadius="10" Background="Transparent"/>
    <Border x:Name="Thumb" Width="12" Height="12" CornerRadius="6" Background="{TemplateBinding Foreground}" HorizontalAlignment="Left" Margin="4,0,0,0"><Border.RenderTransform><TranslateTransform x:Name="Move"/></Border.RenderTransform></Border>
   </Grid>
   <ContentPresenter Grid.Column="1" Margin="4,0,0,0" VerticalAlignment="Center" RecognizesAccessKey="True"/>
  </Grid>
 </Border>
 <ControlTemplate.Triggers>
  <Trigger Property="IsChecked" Value="True"><Setter TargetName="Track" Property="Background" Value="{DynamicResource SMT.AccentBrush}"/><Setter TargetName="Track" Property="BorderBrush" Value="{DynamicResource SMT.AccentBrush}"/><Setter TargetName="Thumb" Property="Background" Value="{DynamicResource SMT.OnAccentBrush}"/><Setter TargetName="Thumb" Property="Margin" Value="22,0,0,0"/></Trigger>
  <Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="Focus" Property="BorderBrush" Value="{DynamicResource {x:Static SystemColors.HighlightBrushKey}}"/></Trigger>
  <Trigger Property="IsEnabled" Value="False"><Setter TargetName="Focus" Property="Opacity" Value="0.45"/></Trigger>
 </ControlTemplate.Triggers>
</ControlTemplate>
""")));return style;}
}
