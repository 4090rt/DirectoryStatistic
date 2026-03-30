using DirectoryStatistic.Http.ModelData;
using DirectoryStatistic.Http.Parser;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.HttpRequest
{
    public class HttpRequestPing
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public HttpRequestPing(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<DataPing>> Request(string host, CancellationToken cancellation = default)
        {
            HttpResponseMessage response = null;
            try
            {
                var client = _httpClientFactory.CreateClient("ClientProvider");

                var options = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/generate_204")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                _logger.LogInformation("Начинаю замер пинга до Google (generate_204)");
                var timer = System.Diagnostics.Stopwatch.StartNew();
                response = await client.SendAsync(options, cancellation);
                if (response.IsSuccessStatusCode)
                {
                    var pingms = timer.ElapsedMilliseconds / 2;

                    return new List<DataPing>
                    {
                        new DataPing()
                        {
                        Host = host,
                        PingMs = pingms,
                        Status = "success",
                        Error = null
                        }
                    };
                }
                else
                {
                    return new List<DataPing>
                    {
                        new DataPing
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "error",
                        Error = $"HTTP {response.StatusCode}"
                        }
                    };
                }
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError(ex, "Операция отменена по таймауту");
                return new List<DataPing>
                    {
                        new DataPing
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "error",
                        Error = $"HTTP {response.StatusCode}"
                        }
                    };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Операция отменена пользователем");
                return new List<DataPing>
                    {
                        new DataPing
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "error",
                        Error = $"HTTP {response.StatusCode}"
                        }
                    };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло исключение при  HTTP запросе" + ex.Message + ex.StackTrace);
                return new List<DataPing>
                    {
                        new DataPing
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "error",
                        Error = $"HTTP {response.StatusCode}"
                        }
                    }; ;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<DataPing>
                    {
                        new DataPing
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "error",
                        Error = $"HTTP {response.StatusCode}"
                        }
                    };
            }
        }


    }
}
