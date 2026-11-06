using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
public sealed partial class Services {
 readonly HttpClient http; public Services(){http=new(){Timeout=TimeSpan.FromSeconds(25)};} internal Services(HttpMessageHandler handler){http=new(handler){Timeout=TimeSpan.FromSeconds(25)};}
 public static void Open(string url){if(Uri.TryCreate(url,UriKind.Absolute,out var u)&&u.Scheme=="https")Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}
 async Task<JsonDocument> Get(string url,string? token=null){using var req=new HttpRequestMessage(HttpMethod.Get,url);if(token!=null)req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);using var r=await http.SendAsync(req);if(!r.IsSuccessStatusCode)throw new InvalidOperationException($"Serviço retornou HTTP {(int)r.StatusCode}.");return JsonDocument.Parse(await r.Content.ReadAsStringAsync());}
 public async Task<List<(string Name,double Lat,double Lon)>> Cities(string name){using var j=await Get("https://geocoding-api.open-meteo.com/v1/search?count=8&language=pt&format=json&name="+Uri.EscapeDataString(name));var result=new List<(string,double,double)>();if(j.RootElement.TryGetProperty("results",out var a))foreach(var e in a.EnumerateArray())result.Add(($"{e.GetProperty("name").GetString()}, {Text(e,"admin1")} · {Text(e,"country")}",e.GetProperty("latitude").GetDouble(),e.GetProperty("longitude").GetDouble()));return result;}
 public async Task<WeatherSnapshot> Weather(Settings s){var lat=s.Latitude.ToString(CultureInfo.InvariantCulture);var lon=s.Longitude.ToString(CultureInfo.InvariantCulture);using var j=await Get($"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,apparent_temperature,weather_code,wind_speed_10m&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto&forecast_days=7");var c=j.RootElement.GetProperty("current");var d=j.RootElement.GetProperty("daily");var days=new List<ForecastDay>();for(int i=0;i<d.GetProperty("time").GetArrayLength();i++)days.Add(new(d.GetProperty("time")[i].GetString()!,d.GetProperty("temperature_2m_min")[i].GetDouble(),d.GetProperty("temperature_2m_max")[i].GetDouble(),d.GetProperty("weather_code")[i].GetInt32(),d.GetProperty("precipitation_probability_max")[i].ValueKind==JsonValueKind.Null?0:d.GetProperty("precipitation_probability_max")[i].GetInt32()));return new(s.City,c.GetProperty("temperature_2m").GetDouble(),c.GetProperty("apparent_temperature").GetDouble(),c.GetProperty("weather_code").GetInt32(),c.GetProperty("wind_speed_10m").GetDouble(),days,DateTimeOffset.Now);}
 internal static string Text(JsonElement e,string p,string fallback="")=>e.TryGetProperty(p,out var v)&&v.ValueKind==JsonValueKind.String?v.GetString()??fallback:fallback;
}
