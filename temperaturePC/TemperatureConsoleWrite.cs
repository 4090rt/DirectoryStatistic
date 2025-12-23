using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.temperaturePC
{
    public class TemperatureConsoleWrite
    {
        public static void PrintTemperatures()
        {
            Console.WriteLine("=== ТЕМПЕРАТУРЫ КОМПОНЕНТОВ ===");
            Console.WriteLine(new string('=', 50));
            try
            {
                Console.WriteLine("=== ТЕМПЕРАТУРЫ КОМПОНЕНТОВ ===");
                Console.WriteLine(new string('=', 50));

                var temp = GetTemperature.Getemp();

                if (temp != null)
                {
                    foreach (var hardware in temp)
                    {
                        Console.WriteLine($"\n{hardware.Key}");
                        Console.WriteLine(new string('-', 30));

                        foreach (var sensor in hardware.Value)
                        {
                            var status = StatusTemperature.GetTemperatureStatus(sensor.Value, sensor.Name);
                            Console.WriteLine($"  {sensor.Name,-20}: {sensor.Value,5:F1}°C  {status}");

                            if (sensor.Min > 0 && sensor.Max > 0)
                            {
                                Console.WriteLine($"    (Диапазон: {sensor.Min:F1} - {sensor.Max:F1}°C)");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка вывола на консоль" + ex.Message);
            }
        }
    }
}
