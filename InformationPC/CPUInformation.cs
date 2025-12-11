using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationPC
{
    public class CPUInformation
    {
        public void CPUINFORM()
        {
            try
            {
                using var search = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                {
                    foreach (var item in search.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"\nПроцессор:  {item["Name"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Мануфактура:  {item["Manufacturer"]}");
                        Console.WriteLine($"Ядер:  {item["NumberOfCores"]}");
                        Console.WriteLine($"Потоков:  {item["NumberOfLogicalProcessors"]}");
                        Console.WriteLine($"Максимальная частота:  {item["MaxClockSpeed"]} MHz"); 
                        Console.WriteLine($"Текущая частота:  {item["CurrentClockSpeed"]} MHz");
                        Console.WriteLine($"Cокет:  {item["SocketDesignation"]} ");
                        Console.WriteLine($"Статус:  {item["Status"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить информацию о процессора"  + ex.Message);
            }
        }
    }
}
