using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
namespace DirectoryStatistic.InformationPC
{
    public class GPUInformation
    {
        public void GPUiNFO()
        {
            try
            {
                using var search = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                {
                    foreach (var item in search.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nВидеокарта: {item["Name"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        var pam = Convert.ToUInt64(item["AdapterRam"]) / (1024 * 1024 * 1024);
                        Console.WriteLine($"Память GB: {pam}");
                        Console.WriteLine($"Версия драйвера: {item["DriverVersion"]}");
                        Console.WriteLine($"Текущий режим:  { item["VideoModeDescription"]}"); 
                        Console.WriteLine($"Частота обновления:  {item["CurrentRefreshRate"]}"); 
                        Console.WriteLine($"Производитель:  {item["AdapterCompatibility"]}");
                        Console.WriteLine($"Статус:  {item["Status"]}");
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Не удалось получить информацию о видеоадаптере" + ex.Message);
            }
        }
    }
}
