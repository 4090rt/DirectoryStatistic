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
        private static double GetUsageCPU(Process process)
        {
            try
            {
                var starttime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                Thread.Sleep(200);

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
        public void ShowTopProcesses(int topCount = 10)
        {
            Console.WriteLine($"=== ТОП-{topCount} ПРОЦЕССОВ ПО ПОТРЕБЛЕНИЮ РЕСУРСОВ ===\n Это может занять некоторое время, подождите");
            Console.WriteLine(new string('=', 60));

            try
            {
                var allprocesses = Process.GetProcesses();

                if (allprocesses.Length == 0)
                {
                    Console.WriteLine("Не удалось получить список процессов");
                    return;
                }
                var topByCPU = allprocesses
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName) && p.Id != 0)
                    .OrderByDescending(p => GetUsageCPU(p))
                    .Take(topCount)
                    .ToList();

                var topByMemory = allprocesses
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName) && p.Id != 0)
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(topCount)
                    .ToList();


                Console.WriteLine("\n📈 ТОП по загрузке CPU:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"{"Процесс",-30} {"PID",-8} {"CPU %",-10} {"Память",-12}");
                Console.WriteLine(new string('-', 60));

                foreach (var process in topByCPU)
                {
                    try
                    {
                        double cpuusage = GetUsageCPU(process);
                        double memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;

                        Console.WriteLine($"{process.ProcessName,-30} {process.Id,-8} {cpuusage,6:F1}% {memoryMB,8:F1} MB");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Ошибка обработки {process.ProcessName}: {ex.Message}");
                        continue;
                    }
                }

                Console.WriteLine("\n💾 ТОП по потреблению памяти:");
                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"{"Процесс",-30} {"PID",-8} {"Память",-12} {"CPU %",-10}");
                Console.WriteLine(new string('-', 60));

                foreach (var process in topByMemory)
                {
                    try
                    {
                        double memoryusage = GetUsageCPU(process);
                        double memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;

                        Console.WriteLine($"{process.ProcessName,-30} {process.Id,-8} {memoryMB,8:F1} MB {memoryusage,6:F1}%");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Ошибка обработки {process.ProcessName}: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при анализе процессов" + ex.Message);
            }
        }

    }

}
