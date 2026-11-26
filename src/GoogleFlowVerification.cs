using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
internal static class GoogleFlowVerification {
 static Dictionary<string,string> Query(string query)=>query.TrimStart('?').Split('&').Select(x=>x.Split('=',2)).ToDictionary(x=>Uri.UnescapeDataString(x[0]),x=>Uri.UnescapeDataString(x[1].Replace('+',' ')));
 internal static async Task<Dictionary<string,bool>> Run(){
  var checks=new Dictionary<string,bool>();var client=new GoogleClient("synthetic.apps.googleusercontent.com","synthetic-desktop-client");
  var handler=new Exchange();var service=new Services(handler);Dictionary<string,string>? request=null;Task? callback=null;
  bool Browser(string url){request=Query(new Uri(url).Query);callback=Task.Run(async()=>{using var http=new HttpClient();using var response=await http.GetAsync(request["redirect_uri"]+"?state="+Uri.EscapeDataString(request["state"])+"&code=synthetic-code");response.EnsureSuccessStatusCode();});return true;}
  var account=await service.Connect(client,Browser,CancellationToken.None);await callback!;
  checks["googleOAuthLoopbackReturnAndIdentity"]=new Uri(request!["redirect_uri"]).IsLoopback&&account.Email=="fixture@example.test"&&account.RefreshToken=="synthetic-refresh";
  checks["googleOAuthPkceAndState"]=request["state"].Length>=40&&request["code_challenge_method"]=="S256"&&request["code_challenge"]==Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(handler.Form!["code_verifier"]))).TrimEnd('=').Replace('+','-').Replace('/','_');
  checks["googleOAuthCodeExchange"]=handler.Form!["grant_type"]=="authorization_code"&&handler.Form["code"]=="synthetic-code"&&handler.Form["redirect_uri"]==request["redirect_uri"];
  try{await service.Connect(client,_=>false,CancellationToken.None);checks["googleBrowserLaunchFailureVisible"]=false;}catch(InvalidOperationException ex){checks["googleBrowserLaunchFailureVisible"]=ex.Message.Contains("navegador");}
  foreach(var scenario in new[]{"denied","wrong-state"}){
   int before=handler.Exchanges;Task? returning=null;
   try{await service.Connect(client,url=>{var q=Query(new Uri(url).Query);returning=Task.Run(async()=>{using var http=new HttpClient();using var response=await http.GetAsync(q["redirect_uri"]+"?state="+(scenario=="wrong-state"?"wrong":q["state"])+"&"+(scenario=="denied"?"error=access_denied":"code=synthetic-code"));});return true;},CancellationToken.None);checks["googleCallback_"+scenario]=false;}
   catch(OperationCanceledException){checks["googleCallback_"+scenario]=scenario=="denied"&&before==handler.Exchanges;}
   catch(InvalidOperationException){checks["googleCallback_"+scenario]=scenario=="wrong-state"&&before==handler.Exchanges;}
   if(returning!=null)await returning;
  }
  using var cancel=new CancellationTokenSource();var pending=service.Connect(client,_=>true,cancel.Token);cancel.Cancel();try{await pending;checks["googlePendingAuthorizationCanCancel"]=false;}catch(OperationCanceledException){checks["googlePendingAuthorizationCanCancel"]=true;}
  return checks;
 }
 sealed class Exchange:HttpMessageHandler {
  internal Dictionary<string,string>? Form;internal int Exchanges;
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req,CancellationToken ct){string body;
   if(req.RequestUri!.Host=="oauth2.googleapis.com"){Exchanges++;Form=Query(await req.Content!.ReadAsStringAsync(ct));body=JsonSerializer.Serialize(new{access_token="synthetic-access",refresh_token="synthetic-refresh",scope="openid email https://www.googleapis.com/auth/calendar.events.readonly https://www.googleapis.com/auth/calendar.calendarlist.readonly"});}
   else if(req.RequestUri.Host=="openidconnect.googleapis.com"&&req.Headers.Authorization?.Parameter=="synthetic-access")body="{\"email\":\"fixture@example.test\"}";
   else throw new InvalidOperationException("Unexpected test endpoint");
   return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body,Encoding.UTF8,"application/json")};
  }
 }
}
