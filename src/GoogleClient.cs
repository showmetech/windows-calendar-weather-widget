using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
namespace CalendarWeather;
public sealed record GoogleClient(string ClientId,string ClientSecret);
public static class GoogleClientStore {
 public const string MissingConfigurationMessage="A conexão Google está indisponível nesta versão: falta a configuração do aplicativo pelo desenvolvedor.";
 internal static GoogleClient? ProductClient(IEnumerable<KeyValuePair<string,string?>> metadata){var values=metadata.ToDictionary(x=>x.Key,x=>x.Value);values.TryGetValue("SMT.GoogleOAuth.DesktopClientId",out var id);values.TryGetValue("SMT.GoogleOAuth.DesktopClientSecret",out var secret);return id?.EndsWith(".apps.googleusercontent.com",StringComparison.Ordinal)==true&&!string.IsNullOrEmpty(secret)?new GoogleClient(id,secret):null;}
 public static GoogleClient? Load(){foreach(var path in new[]{Path.Combine(Store.Root,"google-client.dat"),Path.Combine(AppContext.BaseDirectory,"Assets","google-client.dat")}){try{return JsonSerializer.Deserialize<GoogleClient>(Encoding.UTF8.GetString(Store.Unprotect(File.ReadAllBytes(path))));}catch{}}return ProductClient((Assembly.GetEntryAssembly()??typeof(GoogleClientStore).Assembly).GetCustomAttributes<AssemblyMetadataAttribute>().Select(x=>new KeyValuePair<string,string?>(x.Key,x.Value)));}
 public static void Import(string file){using var doc=JsonDocument.Parse(File.ReadAllText(file));if(!doc.RootElement.TryGetProperty("installed",out var root))throw new InvalidOperationException("O cliente deve ser do tipo App para computador.");var id=Services.Text(root,"client_id");var secret=Services.Text(root,"client_secret");if(!id.EndsWith(".apps.googleusercontent.com",StringComparison.Ordinal)||secret.Length==0)throw new InvalidOperationException("JSON OAuth inválido.");Directory.CreateDirectory(Store.Root);var p=Path.Combine(Store.Root,"google-client.dat");File.WriteAllBytes(p+".tmp",Store.Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new GoogleClient(id,secret)))));File.Move(p+".tmp",p,true);}
}
