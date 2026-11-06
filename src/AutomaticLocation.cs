using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
namespace CalendarWeather;
public record LocationResult(bool Success,string Status,double Latitude=0,double Longitude=0,double Accuracy=0);
public static class AutomaticLocation {
 public static async Task<LocationResult> Detect(bool requestPermission){
 try {
 if(requestPermission){var access=await Geolocator.RequestAccessAsync();if(access!=GeolocationAccessStatus.Allowed)return new(false,"O Windows não autorizou a localização. Você pode permitir o acesso e tentar novamente, ou usar uma cidade manual.");}
 var locator=new Geolocator{DesiredAccuracyInMeters=5000};
 var position=await locator.GetGeopositionAsync(TimeSpan.FromMinutes(15),TimeSpan.FromSeconds(20));var p=position.Coordinate.Point.Position;
 return new(true,"Localização automática do Windows",Math.Round(p.Latitude,3),Math.Round(p.Longitude,3),position.Coordinate.Accuracy);
 }catch(UnauthorizedAccessException){return new(false,"Localização bloqueada pelo Windows. Permita o acesso e tente novamente, ou use uma cidade manual.");}
 catch(Exception){return new(false,"O Windows não forneceu a localização agora. Mantida a cidade anterior; tente novamente ou escolha uma cidade.");}
 }
}
