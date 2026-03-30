using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class TopProcess
    {
        private readonly ILogger<TopProcess> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TopProcess(ILogger<TopProcess> logger, IMemoryCache memoryCache)
        {
            _logger = logger; _memoryCache = memoryCache;
        }
        public async Task<double> CacheUsageCpu(Process process, CancellationToken cancellation = default)
        {
            string cache_key = $"cpu_{process.Id}_{process.ProcessName}";
            string stalecache = $"stale_{cache_key}";

            
            if (_memoryCache.TryGetValue(cache_key, out double cached))
            {
                _logger.LogDebug($"📦 Кэш: {process.ProcessName} = {cached:F1}%");
                return cached;
            }

            await _semaphore.WaitAsync(cancellation);

            try
            {
                if (_memoryCache.TryGetValue(cache_key, out double cached2))
                {
                    return cached2;
                }

                _memoryCache.TryGetValue(stalecache, out double staleValue);

                var cpuValue = await GetUsageCPUAsync(process, cancellation);

                if (cpuValue != 0)
                {
                    var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _memoryCache.Set(cache_key, cpuValue, options);
                    _memoryCache.Set(stalecache, cpuValue, TimeSpan.FromMinutes(15));

                    _logger.LogInformation("✅ Cached fresh data for {CacheCode}", cache_key);
                    return cpuValue;
                }

                if (staleValue != 0)
                {
                    _logger.LogInformation("✅ Returning stale value for {ProcessName}", process.ProcessName);
                    return staleValue;
                }

                _logger.LogWarning("⚠️ Не удалось получить CPU, возвращаю 0 для {ProcessName}", process.ProcessName);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении CPU для {ProcessName}", process.ProcessName);

                if (_memoryCache.TryGetValue(stalecache, out double staleValue))
                {
                    _logger.LogInformation("✅ Returning stale value after error for {ProcessName}", process.ProcessName);
                    return staleValue;
                }

                return 0;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async Task<double> GetUsageCPUAsync(Process process, CancellationToken cancellation = default)
        {
            try
            {
                var starttime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                await Task.Delay(100, cancellation);

                var endtime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalPassed = (endtime - starttime).TotalMilliseconds;
                var cpuUsagePercent = (cpuUsedMs / totalPassed) * 100;

                return cpuUsagePercent;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetUsageCPU(Process process)
        {
            try
            {
                var starttime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                Thread.Sleep(100);

                var endtime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalPassed = (endtime - starttime).TotalMilliseconds;
                var cpuUsagePercent = (cpuUsedMs / totalPassed) * 100;

                return cpuUsagePercent;
            }
            catch
            {
                return 0;
            }
        }
        public async Task ShowTopProcessesAsync(int topCount = 10, CancellationToken cancellation = default)
        {
            Console.WriteLine($"=== ТОП-{topCount} ПРОЦЕССОВ ПО ПОТРЕБЛЕНИЮ РЕСУРСОВ ===\n Это может занять некоторое время, подождите");
            Console.WriteLine(new string('=', 60));

            try
            {
                var allprocesses = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName) && p.Id != 0)
                    .ToList();

                if (allprocesses.Count == 0)
                {
                    Console.WriteLine("Не удалось получить список процессов");
                    return;
                }

                // Предварительно вычисляем CPU для всех процессов с использованием кэша
                var processesWithCpu = new List<(Process Process, double Cpu, long Memory)>();
                foreach (var process in allprocesses)
                {
                    try
                    {
                        var cpu = await CacheUsageCpu(process, cancellation);
                        var memory = process.WorkingSet64;
                        processesWithCpu.Add((process, cpu, memory));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Ошибка получения данных для процесса {ProcessName}", process.ProcessName);
                    }
                }

                // Сортируем по CPU
                var topByCPU = processesWithCpu
                    .OrderByDescending(x => x.Cpu)
                    .Take(topCount)
                    .ToList();

                // Сортируем по памяти
                var topByMemory = processesWithCpu
                    .OrderByDescending(x => x.Memory)
                    .Take(topCount)
                    .ToList();

                // Вывод ТОП по CPU
                Console.WriteLine("\n📈 ТОП по загрузке CPU:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"{"Процесс",-30} {"PID",-8} {"CPU %",-10} {"Память",-12}");
                Console.WriteLine(new string('-', 60));

                foreach (var item in topByCPU)
                {
                    try
                    {
                        double memoryMB = item.Memory / 1024.0 / 1024.0;
                        Console.WriteLine($"{item.Process.ProcessName,-30} {item.Process.Id,-8} {item.Cpu,6:F1}% {memoryMB,8:F1} MB");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Ошибка обработки {item.Process.ProcessName}: {ex.Message}");
                        continue;
                    }
                }

                // Вывод ТОП по памяти
                Console.WriteLine("\n💾 ТОП по потреблению памяти:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"{"Процесс",-30} {"PID",-8} {"Память",-12} {"CPU %",-10}");
                Console.WriteLine(new string('-', 60));

                foreach (var item in topByMemory)
                {
                    try
                    {
                        double memoryMB = item.Memory / 1024.0 / 1024.0;
                        Console.WriteLine($"{item.Process.ProcessName,-30} {item.Process.Id,-8} {memoryMB,8:F1} MB {item.Cpu,6:F1}%");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Ошибка обработки {item.Process.ProcessName}: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Операция отменена пользователем");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при анализе процессов");
                Console.WriteLine($"Ошибка при анализе процессов: {ex.Message}");
            }
        }

    }

}
