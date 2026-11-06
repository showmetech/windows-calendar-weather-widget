using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
internal sealed class CalendarTestHandler:HttpMessageHandler {
 public bool Refreshed;public bool WrongMethod;
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken){var path=request.RequestUri!.AbsolutePath;string payload;
 if(path=="/token"){var body=await request.Content!.ReadAsStringAsync(cancellationToken);Refreshed=request.Method==HttpMethod.Post&&body.Contains("grant_type=refresh_token")&&body.Contains("refresh_token=synthetic-refresh");payload="{\"access_token\":\"synthetic-access\"}";}
 else if(path.EndsWith("calendarList")){WrongMethod|=request.Method!=HttpMethod.Get;payload="{\"items\":[{\"id\":\"primary\",\"summary\":\"Calendário de teste\",\"backgroundColor\":\"#80AAFF\"}]}";}
 else{WrongMethod|=request.Method!=HttpMethod.Get;payload="{\"items\":[{\"id\":\"example\",\"summary\":\"Evento sintético\",\"start\":{\"dateTime\":\""+DateTimeOffset.Now.AddHours(1).ToString("o")+"\"},\"end\":{\"dateTime\":\""+DateTimeOffset.Now.AddHours(2).ToString("o")+"\"}}]}";}
 return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(payload,Encoding.UTF8,"application/json")};}
}
internal static class ReconnectTests {
 public static async Task<bool> Run(){var account=new Account{ClientId="synthetic-client",ClientSecret="synthetic-secret",Email="teste fictício",RefreshToken="synthetic-refresh"};Store.SaveAccounts(new(){account});var restored=Store.Accounts().Single();var handler=new CalendarTestHandler();var calendar=await new Services(handler).Calendar(restored);Store.SaveAccounts(new());return handler.Refreshed&&!handler.WrongMethod&&calendar.Events.Count==1&&calendar.Calendars.Count==1;}
}
