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

            try
            {
                Console.WriteLine("=== ТЕМПЕРАТУРЫ КОМПОНЕНТОВ ===");
                Console.WriteLine(new string('=', 50));

                var temp = GetTemperature.Getemp();

                if (temp != null)
                {
                    if (temp.Count == 0)
                    {
                        Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: Словарь пустой. Возможные причины:");
                        Console.WriteLine("1. Нет доступных датчиков температуры");
                        Console.WriteLine("2. Библиотека не инициализирована");
                        Console.WriteLine("3. Нет прав администратора");
                        Console.WriteLine("4. Ошибка в методе GetTemperature.Getemp()");
                        return;
                    }
                    foreach (var hardware in temp)
                    {
                        Console.WriteLine($"\n{hardware.Key}");
                        Console.WriteLine(new string('-', 30));

                        foreach (var sensor in hardware.Value)
                        {
                            var status = StatusTemperature.GetTemperatureStatus(sensor.Value, sensor.Name);
                            Console.WriteLine($"  {sensor.Name,-20}: {sensor.Value,5:F1}°C  {status}");

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
