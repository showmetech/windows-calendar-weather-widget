using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
public sealed partial class Services {
 static readonly SemaphoreSlim nameGate=new(1,1);static DateTimeOffset lastNameRequest=DateTimeOffset.MinValue;
 internal static string FormatLocationName(JsonElement root){if(!root.TryGetProperty("address",out var address))return "";var town=new[]{"city","town","municipality","village","county"}.Select(key=>Text(address,key)).FirstOrDefault(x=>!string.IsNullOrWhiteSpace(x))??"";var state=Text(address,"state");var code=Text(address,"ISO3166-2-lvl4");if(code.StartsWith("BR-",StringComparison.Ordinal))state=code[3..];var country=Text(address,"country");var region=town;if(!string.IsNullOrEmpty(state)&&!string.Equals(state,town,StringComparison.OrdinalIgnoreCase))region=string.IsNullOrEmpty(region)?state:region+", "+state;return string.IsNullOrEmpty(country)?region:string.IsNullOrEmpty(region)?country:region+" · "+country;}
 public async Task<string> LocationName(double latitude,double longitude){await nameGate.WaitAsync();try{var wait=TimeSpan.FromMilliseconds(1100)-(DateTimeOffset.UtcNow-lastNameRequest);if(wait>TimeSpan.Zero)await Task.Delay(wait);lastNameRequest=DateTimeOffset.UtcNow;var lat=Math.Round(latitude,2).ToString("F2",CultureInfo.InvariantCulture);var lon=Math.Round(longitude,2).ToString("F2",CultureInfo.InvariantCulture);using var request=new HttpRequestMessage(HttpMethod.Get,"https://nominatim.openstreetmap.org/reverse?format=jsonv2&zoom=10&addressdetails=1&accept-language=pt-BR&lat="+lat+"&lon="+lon);request.Headers.UserAgent.ParseAdd("WindowsCalendarWeatherWidget/1.2.0 (personal desktop widget)");using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(10));using var response=await http.SendAsync(request,timeout.Token);response.EnsureSuccessStatusCode();using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));return FormatLocationName(json.RootElement);}finally{nameGate.Release();}}
}
