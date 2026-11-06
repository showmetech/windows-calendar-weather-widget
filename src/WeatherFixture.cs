using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
internal sealed class WeatherFixture:HttpMessageHandler {
 protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken){
  if(request.RequestUri?.Host!="api.open-meteo.com")throw new InvalidOperationException("Unexpected network request in offline test.");
  var body=new{current=new{temperature_2m=22.5,apparent_temperature=23.0,weather_code=2,wind_speed_10m=10.0},daily=new{time=Enumerable.Range(0,7).Select(i=>DateTime.Today.AddDays(i).ToString("yyyy-MM-dd")),temperature_2m_min=Enumerable.Repeat(18.0,7),temperature_2m_max=Enumerable.Repeat(25.0,7),weather_code=Enumerable.Repeat(2,7),precipitation_probability_max=Enumerable.Repeat(20,7)}};
  return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json")});
 }
}
