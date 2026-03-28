using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationPC
{
    public class RAMInformation
    {
        public void RAMINFORMATIO()
        {
            try
            {
                using var information = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                {
                    foreach (ManagementObject obj in information.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Оперативная память");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Объем одной планки:"); var sizeGB = Convert.ToUInt64(obj["Capacity"]) / (1024 * 1024 * 1024);
                        Console.WriteLine($"Планка: {obj["DeviceLocator"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Объем: {sizeGB}GB");
                        Console.WriteLine($"Частота: {obj["Speed"]}MHz");
                        Console.WriteLine($"Производитель: {obj["Manufacturer"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить информацию о оперативной памяти" + ex.Message);
            }
        }
    }
}
