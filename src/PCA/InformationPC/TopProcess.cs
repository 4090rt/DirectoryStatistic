using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationPC
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

            try
            {
                var cpuValue = await GetUsageCPUAsync(process, cancellation);
                return cpuValue;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ошибка получения CPU для {ProcessName}", process.ProcessName);
                return 0;
            }
        }

        private async Task<double> GetUsageCPUAsync(Process process, CancellationToken cancellation = default)
        {
            try
            {
                bool hasExited;
                try
                {
                    hasExited = process.HasExited;
                }
                catch (Win32Exception)
                {
                    _logger.LogDebug("Нет доступа к процессу {ProcessName} (PID: {Pid})", process.ProcessName, process.Id);
                    return 0;
                }

                if (hasExited)
                {
                    _logger.LogDebug("Процесс {ProcessName} (PID: {Pid}) завершился до замера CPU", process.ProcessName, process.Id);
                    return 0;
                }

                var processorCount = Environment.ProcessorCount;
                var startTime = DateTime.UtcNow;
                
                TimeSpan startCpuUsage;
                try
                {
                    startCpuUsage = process.TotalProcessorTime;
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
                {
                    _logger.LogDebug("Нет доступа к TotalProcessorTime для {ProcessName} (PID: {Pid})", process.ProcessName, process.Id);
                    return 0;
                }

                await Task.Delay(100, cancellation);

                try
                {
                    hasExited = process.HasExited;
                }
                catch (Win32Exception)
                {
                    _logger.LogDebug("Потерян доступ к процессу {ProcessName} (PID: {Pid}) во время замера", process.ProcessName, process.Id);
                    return 0;
                }

                if (hasExited)
                {
                    _logger.LogDebug("Процесс {ProcessName} (PID: {Pid}) завершился во время замера CPU", process.ProcessName, process.Id);
                    return 0;
                }

                var endTime = DateTime.UtcNow;
                
                TimeSpan endCpuUsage;
                try
                {
                    endCpuUsage = process.TotalProcessorTime;
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
                {
                    _logger.LogDebug("Потерян доступ к TotalProcessorTime для {ProcessName} (PID: {Pid})", process.ProcessName, process.Id);
                    return 0;
                }

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalPassedMs = (endTime - startTime).TotalMilliseconds;

                if (totalPassedMs == 0)
                {
                    return 0;
                }

                // Нормализуем на количество ядер: 100% = все ядра загружены на 100%
                var cpuUsagePercent = (cpuUsedMs / totalPassedMs) * 100 / processorCount;

                return cpuUsagePercent;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "Процесс {ProcessName} (PID: {Pid}) недоступен", process.ProcessName, process.Id);
                return 0;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
            {
                _logger.LogDebug("Отказано в доступе к процессу {ProcessName} (PID: {Pid})", process.ProcessName, process.Id);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при замере CPU для {ProcessName} (PID: {Pid})", process.ProcessName, process.Id);
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
