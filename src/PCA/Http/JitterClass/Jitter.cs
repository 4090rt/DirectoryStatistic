using DirectoryStatistic.Http.HttpRequest;
using DirectoryStatistic.Http.ModelData;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.JitterClass
{
    public class Jitter
    {
        private readonly ILogger<Jitter> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpRequestPing _httpRequestPing;
        public Jitter(ILogger<Jitter> logger, IHttpClientFactory httpClientFactory, HttpRequestPing httpRequestPing)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpRequestPing = httpRequestPing;
        }

        public async Task<List<long>> RequestPing(string host, int colvo, CancellationToken cancellation = default)
        {
            var ResultPingRequests = new List<long>();
            try
            {
                _logger.LogInformation("Начинаю вызов ping запросов для вычисления отклонения");

                int co = 0;
                int countlogger = 0;
                while (co < colvo)
                {
                    var result = await _httpRequestPing.Request(host,cancellation);
                    await Task.Delay(2000);
                    if (result != null)
                    {

                        foreach (var items in result)
                        {
                            var ping = items.PingMs;
                            if (ping > 0)
                            {
                                ResultPingRequests.Add(ping);
                                _logger.LogInformation($"Успешно добавлено  {countlogger}");
                            }
                            else
                            {
                                _logger.LogWarning("Ping пуст! Количество успешных запросов:" + countlogger);
                                return new List<long>();
                            }
                        }
                    }
                    else
                    {

                        _logger.LogWarning($"Результат запрос {countlogger} пуст!");
                        return new List<long>();
                    }
                    co++;
                    countlogger++;
                }
                return ResultPingRequests;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);
                return new List<long>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло исключение при совершении запросов" + ex.Message + ex.StackTrace);
                return new List<long>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<long>();
            }
        }


        public async Task<DataJitter> JitterSc(string host, int colvo, CancellationToken cancellation = default)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var resultlist = await RequestPing(host, colvo);
            timer.Stop();
            try
            {
                int count = 0;
                long MaxMs = 0;
                long MinxMs = 0;
                double Average = 0;
                double Jitterres = 0;
                double timerr = timer.ElapsedMilliseconds;

                _logger.LogInformation("Начинаю расчет среднего отклонения занчений пинга между замерами");
                if (resultlist != null && resultlist.Count > 0)
                {
                    count = resultlist.Count;
                    MaxMs = resultlist.Max();
                    MinxMs = resultlist.Min();
                    Average = resultlist.Average();

                    var difference = new List<long>();

                    for (int i = 1; i < resultlist.Count; i++)
                    {
                        difference.Add(Math.Abs(resultlist[i] - resultlist[i - 1]));
                    }
                    Jitterres = difference.Average();

                    _logger.LogInformation("Расчет сделан успешно");
                    return new DataJitter
                    {
                        Count = count,
                        MaxMs = MaxMs,
                        MinMS = MinxMs,
                        Average = Average,
                        JitterMs = Jitterres,
                        Timer = timerr
                    };
                }
                else
                {
                    _logger.LogWarning("Список пинга пуст!");
                    throw new InvalidOperationException("Пустой список");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                throw new InvalidOperationException("Исключение при расчете");
            }
        }
    }
}
