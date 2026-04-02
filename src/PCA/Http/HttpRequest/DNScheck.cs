using DirectoryStatistic.Http.ModelData;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.HttpRequest
{
    public class DNScheck
    {
        private readonly ILogger<DNScheck> _logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);
        private readonly IMemoryCache _memoryCache;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        public DNScheck(ILogger<DNScheck> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<DataResult> CacheRequest(string host, CancellationToken cancellation = default)
        {
            string cachekey = $"cachekey{host}";
            DataResult oldcache = null;
            string stalekey = $"stale{cachekey}";
            if (_memoryCache.TryGetValue(cachekey, out DataResult cached))
            {
                oldcache = cached;
                return cached;
            }
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_memoryCache.TryGetValue(cachekey, out DataResult cached2))
                {
                    return cached2;
                }

                var fallback = Policy<DataResult>
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
                        if (_memoryCache.TryGetValue(stalekey, out DataResult stalecached))
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
                    var result = await Request(host, cancellation);

                    if (result != null)
                    {
                        var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                        _memoryCache.Set(cachekey, result, options);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

                        _memoryCache.Set(stalekey, result, staleoptions);
                        _logger.LogInformation("✅ Cached fresh data for {CacheCode}", cachekey);
                        return result;
                    }
                    else
                    {
                        _logger.LogInformation("✅ Using cached data for {CacheCode}", cachekey);
                        return default;
                    }
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Возникло исключение" + ex.Message + ex.StackTrace);
                return new DataResult();
            }
            finally
            { 
                _semaphoreSlim.Release();
            }
        }

        public async Task<DataResult> Request(string host, CancellationToken cancellation = default)
        {
            try
            {
                var ctx = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                ctx.CancelAfter(_timeout);

                var timer = System.Diagnostics.Stopwatch.StartNew();
                var result = await Dns.GetHostAddressesAsync(host, ctx.Token).ConfigureAwait(false);
                timer.Stop();
                if (result != null && result.Length > 0)
                {
                    var ipv4 = result.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToArray();
                    var ipv6 = result.Where(a => a.AddressFamily == AddressFamily.InterNetworkV6).ToArray();

                    return new DataResult
                    {
                        Host = host,
                        Addresses = result,
                        IPv4 = ipv4.Select(a => a.ToString()).ToArray(),
                        IPv6 = ipv6.Select(a => a.ToString()).ToArray(),
                        ResolveTime = timer.ElapsedMilliseconds,
                        Success = true,
                        Error = null
                    };
                }
                else
                {
                    _logger.LogError("результат null");

                    return new DataResult
                    {
                        Host = host,
                        Addresses = null,
                        IPv4 = Array.Empty<string>(),
                        IPv6 = Array.Empty<string>(),
                        ResolveTime = 0,
                        Success = false,
                        Error = "null result"
                    };
                }
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);

                return new DataResult
                {
                    Host = host,
                    Addresses = null,
                    IPv4 = Array.Empty<string>(),
                    IPv6 = Array.Empty<string>(),
                    ResolveTime = 0,
                    Success = false,
                    Error = ex.Message
                };
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена пользователем" + ex.Message + ex.StackTrace);

                return new DataResult
                {
                    Host = host,
                    Addresses = null,
                    IPv4 = Array.Empty<string>(),
                    IPv6 = Array.Empty<string>(),
                    ResolveTime = 0,
                    Success = false,
                    Error = ex.Message
                };
            }
            catch (SocketException ex)
            {
                _logger.LogError("Возникло исключение сети" + ex.Message + ex.StackTrace);

                return new DataResult
                {
                    Host = host,
                    Addresses = null,
                    IPv4 = Array.Empty<string>(),
                    IPv6 = Array.Empty<string>(),
                    ResolveTime = 0,
                    Success = false,
                    Error = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключени" + ex.Message + ex.StackTrace);

                return new DataResult
                {
                    Host = host,
                    Addresses = null,
                    IPv4 = Array.Empty<string>(),
                    IPv6 = Array.Empty<string>(),
                    ResolveTime = 0,
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}
