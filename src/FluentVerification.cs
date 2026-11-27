using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
namespace CalendarWeather;
internal static class FluentVerification {
 internal static Dictionary<string,bool> Run(SettingsPanel panel,MainWindow main){
  var results=new Dictionary<string,bool>();
  results["threeAccessiblePrimarySections"]=panel.PrimarySectionCount==3&&QualityVerification.Find<TabItem>(panel).Count()==3;
  bool contrast=true;for(int r=0;r<256;r+=17)for(int g=0;g<256;g+=17)for(int b=0;b<256;b+=17){var color=Color.FromRgb((byte)r,(byte)g,(byte)b);contrast&=FluentDesign.Contrast(((SolidColorBrush)FluentDesign.OnAccent(color)).Color,color)>=4.5;}
  results["accentTextContrast4096Colors"]=contrast;
  results["secondaryTextContrastBothThemes"]=new[]{false,true}.All(light=>FluentDesign.Contrast(((SolidColorBrush)FluentDesign.Secondary(light)).Color,((SolidColorBrush)FluentDesign.Surface(light)).Color)>=4.5);
  var preference=main.settings.ReduceMotion;try{main.settings.ReduceMotion=true;results["reduceMotionOverridesWindowsAnimation"]=!FluentDesign.Motion(main.settings);}finally{main.settings.ReduceMotion=preference;}
  results["opaqueByDefault"]=new Settings().OpacityPercent==100;
  double oldOpacity=main.settings.OpacityPercent;try{main.settings.OpacityPercent=100;main.ApplyOpacity();bool opaque=LiveProbe.SurfaceMatchesPreference(main,1);main.settings.OpacityPercent=35;main.ApplyOpacity();double expected=SystemParameters.HighContrast?1:.35;results["liveProbeRespectsOpaqueAndCustomOpacity"]=opaque&&LiveProbe.SurfaceMatchesPreference(main,expected)&&!LiveProbe.SurfaceMatchesPreference(main,expected==1?.35:1);}finally{main.settings.OpacityPercent=oldOpacity;main.ApplyOpacity();}
  bool boundsFit=true,above=false,overlay=false;foreach(double scale in new[]{1.0,1.25,1.5,2.0})foreach(var size in new[]{(320,480),(1280,600),(1920,1080),(2560,1440)}){int w=(int)(size.Item1*scale),h=(int)(size.Item2*scale),left=-w,top=-100;var p=SuitePlacement.Bounds(left,top,0,top+h,(int)(-372*scale),top+h-(int)(152*scale),(int)(360*scale),scale);boundsFit&=p.Left>=left&&p.Top>=top&&p.Left+p.Width<=0&&p.Top+p.Height<=top+h&&p.Width>0&&p.Height>0;if(size.Item2==1440)above|=p.Above;if(size.Item2==480)overlay|=!p.Above;}
  results["settingsBoundsMixedDpiAndNegativeMonitorOrigin"]=boundsFit&&above&&overlay;
  panel.SelectPage(0);panel.UpdateLayout();var toggles=QualityVerification.Find<CheckBox>(panel).Where(c=>c.IsVisible).ToArray();
  results["settingsSwitchesHaveNamesAndThemeContrast"]=toggles.Length>=2&&toggles.All(c=>!string.IsNullOrWhiteSpace(AutomationProperties.GetName(c))&&c.Foreground is SolidColorBrush fg&&main.Foreground is SolidColorBrush expected&&fg.Color==expected.Color);
  return results;
 }
}
