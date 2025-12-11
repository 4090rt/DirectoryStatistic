using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class HTTPZapr
    {
        string Url = "https://ipinfo.io/json";
        private readonly object _lock = new object();
        public async Task Httpzapros()
        { 
            HTTPPool pool = new HTTPPool();
            HttpClient client = null;

            try
            {
                client = pool.httpClientOpen();

                HttpResponseMessage recpon = await client.GetAsync(Url).ConfigureAwait(false);
                recpon.EnsureSuccessStatusCode();

                    var result = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var deserializerresult = JsonSerializer.Deserialize<DeserializeZaprosClass>(result);
                    Type type = deserializerresult.GetType();
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    string text = "";
                    Parallel.ForEach(properties, prop =>
                    {
                        var value = prop.GetValue(deserializerresult);
                        lock (_lock)
                        { 
                          text += $"{prop.Name}: {value} ";
                        }
                    });
                    if (deserializerresult != null)
                    {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"IP: {deserializerresult.ip}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Hostname: { deserializerresult.hostname}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"City: {deserializerresult.city}");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Region: {deserializerresult.region}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Country: {deserializerresult.country}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Loc: {deserializerresult.loc}");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Org: {deserializerresult.org}");
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"Postal: {deserializerresult.postal}");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"Timezone: {deserializerresult.timezone}");
                    Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.WriteLine("Результаты не найдены");
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение:" + ex.Message);
            }
            finally
            {
                pool.httpClientClosed(client);
            }
        }
    }
}
