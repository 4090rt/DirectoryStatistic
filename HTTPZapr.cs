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
                        string resultatfinaly = ($"IP:{deserializerresult.ip}\nHostname:{deserializerresult.hostname}\nCity:{deserializerresult.city}\n" +
                        $"Region:{deserializerresult.region}\nCountry:{deserializerresult.country}\nLoc: {deserializerresult.loc}\n" +
                        $"Org:{deserializerresult.org}\nPostal:{deserializerresult.postal}\nTimezone:{deserializerresult.timezone}\n");
                        Console.WriteLine(resultatfinaly);
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
