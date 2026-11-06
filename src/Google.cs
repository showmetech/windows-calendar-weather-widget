using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace CalendarWeather;
public sealed partial class Services {
 public async Task<Account> Connect(string file,CancellationToken ct){
 if(!string.IsNullOrEmpty(file))GoogleClientStore.Import(file);
 var client=GoogleClientStore.Load()??throw new InvalidOperationException("O cliente Google ainda não foi configurado.");var id=client.ClientId;var secret=client.ClientSecret;
 var verifier=Base64(RandomNumberGenerator.GetBytes(48));var state=Base64(RandomNumberGenerator.GetBytes(32));
 var probe=new TcpListener(IPAddress.Loopback,0);probe.Start();var port=((IPEndPoint)probe.LocalEndpoint).Port;probe.Stop();
 var redirect=$"http://127.0.0.1:{port}/";using var listener=new HttpListener();listener.Prefixes.Add(redirect);listener.Start();
 var challenge=Base64(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
 var scope="openid email https://www.googleapis.com/auth/calendar.calendarlist.readonly https://www.googleapis.com/auth/calendar.events.readonly";
 Open("https://accounts.google.com/o/oauth2/v2/auth?"+Query(new(){["client_id"]=id,["redirect_uri"]=redirect,["response_type"]="code",["scope"]=scope,["access_type"]="offline",["prompt"]="consent select_account",["state"]=state,["code_challenge"]=challenge,["code_challenge_method"]="S256"}));
 var ctx=await listener.GetContextAsync().WaitAsync(TimeSpan.FromMinutes(3),ct);
 var valid=ctx.Request.QueryString["state"]==state;var code=ctx.Request.QueryString["code"];
 var html=Encoding.UTF8.GetBytes("<meta charset=utf-8><title>Calendar e Weather Widget</title><p>Volte ao aplicativo para conferir o resultado da conexão.</p>");
 ctx.Response.ContentType="text/html; charset=utf-8";await ctx.Response.OutputStream.WriteAsync(html,ct);ctx.Response.Close();
 if(!valid||string.IsNullOrEmpty(code))throw new InvalidOperationException("Autorização cancelada ou resposta inválida.");
 using var t=await Post(new(){["client_id"]=id,["client_secret"]=secret,["code"]=code,["code_verifier"]=verifier,["redirect_uri"]=redirect,["grant_type"]="authorization_code"});
 var token=t.RootElement.GetProperty("access_token").GetString()!;var granted=Text(t.RootElement,"scope").Split(' ');
 if(!granted.Contains("https://www.googleapis.com/auth/calendar.events.readonly")||!granted.Contains("https://www.googleapis.com/auth/calendar.calendarlist.readonly"))throw new InvalidOperationException("Autorize a leitura dos calendários e eventos para conectar.");
 if(string.IsNullOrEmpty(Text(t.RootElement,"refresh_token")))throw new InvalidOperationException("O Google não forneceu acesso persistente. Tente conectar novamente.");
 using var user=await Get("https://openidconnect.googleapis.com/v1/userinfo",token);
 return new Account{Email=Text(user.RootElement,"email"),ClientId=id,ClientSecret=secret,RefreshToken=Text(t.RootElement,"refresh_token")};}
 static string Base64(byte[] b)=>Convert.ToBase64String(b).TrimEnd('=').Replace('+','-').Replace('/','_');
 static string Query(Dictionary<string,string> d)=>string.Join("&",d.Select(p=>Uri.EscapeDataString(p.Key)+"="+Uri.EscapeDataString(p.Value)));
 async Task<JsonDocument> Post(Dictionary<string,string> data){using var r=await http.PostAsync("https://oauth2.googleapis.com/token",new FormUrlEncodedContent(data));if(!r.IsSuccessStatusCode)throw new InvalidOperationException($"Google OAuth HTTP {(int)r.StatusCode}. Reconecte a conta ou confira o cliente OAuth.");return JsonDocument.Parse(await r.Content.ReadAsStringAsync());}
 public async Task<CalendarSnapshot> Calendar(Account a){if(a.Provider!="Google")return await Subscription(a,CancellationToken.None);
 using var token=await Post(new(){["client_id"]=a.ClientId,["client_secret"]=a.ClientSecret,["refresh_token"]=a.RefreshToken,["grant_type"]="refresh_token"});
 var bearer=token.RootElement.GetProperty("access_token").GetString()!;var calendars=new List<CalendarInfo>();string page="";
 do{using var j=await Get("https://www.googleapis.com/calendar/v3/users/me/calendarList?maxResults=250&pageToken="+Uri.EscapeDataString(page),bearer);
 foreach(var c in j.RootElement.GetProperty("items").EnumerateArray())calendars.Add(new(a.Key+"|"+Text(c,"id"),Text(c,"summary"),Text(c,"backgroundColor","#8bb9ff")));
 page=Text(j.RootElement,"nextPageToken");}while(page!="");
 var events=new List<AgendaEvent>();foreach(var cal in calendars){page="";do{
 var raw=cal.Id[(a.Key.Length+1)..];var url="https://www.googleapis.com/calendar/v3/calendars/"+Uri.EscapeDataString(raw)+"/events?singleEvents=true&orderBy=startTime&maxResults=250&timeMin="+Uri.EscapeDataString(DateTimeOffset.Now.AddDays(-1).ToString("o"))+"&timeMax="+Uri.EscapeDataString(DateTimeOffset.Now.AddDays(14).ToString("o"))+"&pageToken="+Uri.EscapeDataString(page);
 using var j=await Get(url,bearer);if(j.RootElement.TryGetProperty("items",out var items))foreach(var e in items.EnumerateArray()){
 if(Text(e,"status")=="cancelled"||!e.TryGetProperty("start",out var start)||!e.TryGetProperty("end",out var end))continue;
 bool all=start.TryGetProperty("date",out _);events.Add(new(Text(e,"id"),cal.Id,cal.Name,cal.Color,Text(e,"summary","(Sem título)"),ParseDate(start),ParseDate(end),all,Text(e,"htmlLink")));}
 page=Text(j.RootElement,"nextPageToken");}while(page!="");}
 return new(Store.Upcoming(events,DateTimeOffset.Now),calendars,DateTimeOffset.Now);}
 internal static DateTimeOffset ParseDate(JsonElement e){if(e.TryGetProperty("dateTime",out var v))return DateTimeOffset.Parse(v.GetString()!,CultureInfo.InvariantCulture);var dt=DateTime.ParseExact(e.GetProperty("date").GetString()!,"yyyy-MM-dd",CultureInfo.InvariantCulture);return new DateTimeOffset(dt,TimeZoneInfo.Local.GetUtcOffset(dt));}
}
