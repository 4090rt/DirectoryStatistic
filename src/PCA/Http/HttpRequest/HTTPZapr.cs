using DirectoryStatistic.Http.ModelData;
using DirectoryStatistic.Http.Parser;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.HttpRequest
{
    public class HTTPZapr
    {
        string Url = "https://ipinfo.io/json";
        private readonly object _lock = new object();

        private readonly ILogger _logger;
        private readonly IMemoryCache _memorycache;
        private readonly Parsing _parse;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public HTTPZapr(ILogger logger, IMemoryCache memoryCache, Parsing parsing, IHttpClientFactory httpClientFactory, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _memorycache = memoryCache;
            _parse = parsing;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<List<DataProviderInfo>> RequestCaching()
        { 

        }
        public async Task<List<DataProviderInfo>> Request(CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ClientProvider");

                var clientoptions = new HttpRequestMessage(HttpMethod.Get, "https://ipinfo.io/json")
                {
                   Version = HttpVersion.Version20,
                   VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                using var resuest = await client.SendAsync(clientoptions, cancellation).ConfigureAwait(false);
                if (resuest.IsSuccessStatusCode)
                {
                    var read = await resuest.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    var parse = await _parse.ParsingForHttp<DataProviderInfo>(read).ConfigureAwait(false);

                    return parse;
                }
                else
                {
                    _logger.LogError($"Возникла ошибка запрос. Статус код" + resuest.StatusCode);
                    return new List<DataProviderInfo>();
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Операция отменена");
                return new List<DataProviderInfo>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Возникло исключение во время запроса");
                return new List<DataProviderInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ВОзникло исключение");
                return new List<DataProviderInfo>();
            }
        } 
    }
}
