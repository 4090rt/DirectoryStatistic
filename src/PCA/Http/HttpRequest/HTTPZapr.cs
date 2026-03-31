using DirectoryStatistic.Http.ModelData;
using DirectoryStatistic.Http.Parser;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
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

        private readonly ILogger<HTTPZapr> _logger;
        private readonly IMemoryCache _memorycache;
        private readonly Parsing _parse;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public HTTPZapr(ILogger<HTTPZapr> logger, IMemoryCache memoryCache, Parsing parsing, IHttpClientFactory httpClientFactory, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _memorycache = memoryCache;
            _parse = parsing;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<List<DataProviderInfo>> RequestCaching(CancellationToken cancellation = default)
        {
            string key_cache = $"key_cached";
            string stalecache = $"stale{key_cache}";
            List<DataProviderInfo> oldcache = null;
            if (_memorycache.TryGetValue(key_cache, out List<DataProviderInfo> cached))
            {
                string log = $"📦 Данные из кэша для {cached}";
                oldcache = cached;
                return cached;
            }
            await _semaphore.WaitAsync(cancellation);
            try
            {
                if (_memorycache.TryGetValue(key_cache, out List<DataProviderInfo> cached2))
                {
                    return cached2;
                }

                var fallback = Policy<List<DataProviderInfo>>
                    .Handle<Exception>()
                    .OrResult(r => r == null)
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var exception = outcome.Exception;
                        var IsEmpty = outcome.Result == null;

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by exception: {exception.Message}");
                        }              
                        if (IsEmpty)
                        {
                            _logger.LogWarning($"⚠️ Fallback by empty result");
                        }
                        if (oldcache != null)
                        {
                            _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        if (_memorycache.TryGetValue(stalecache, out List<DataProviderInfo> stalecached))
                        {
                            _logger.LogInformation($"✅ Returning stale copy for {stalecached}");
                            return stalecached;
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                            return default;
                        }
                    },
                    onFallbackAsync: async (outcome, ctx) =>
                    {
                        _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                        await Task.CompletedTask;
                    });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await Request(cancellation);

                    if (result != null)
                    {
                        var options = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                        _memorycache.Set(key_cache,result, options);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                        _logger.LogInformation("✅ Cached fresh data for {CacheCode}", key_cache);
                        _memorycache.Set(stalecache, result, staleoptions);
                        return result;
                    }
                    else
                    {
                        _logger.LogInformation("✅ Using cached data for {CacheCode}", key_cache);
                        return default;
                    }
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Возникло исключение" + ex.Message, ex.StackTrace);
                return new List<DataProviderInfo>();
            }
            finally
            { 
                _semaphore.Release();
            }
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
