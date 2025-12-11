using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationPC
{
    public class DriveInfo
    {
        public void DRIVEIBFO()
        {
            try
            {
                using var information = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                {
                    foreach (var item in information.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"\nСистемный Диск:  {item["Model"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        var pam = Convert.ToUInt64(item["Size"]) / (1024 * 1024 * 1024);
                        Console.WriteLine($"Память GB: {pam}");
                        Console.WriteLine($"Разделов:  {item["Partitions"]}");
                        Console.WriteLine($"Тип интерфейса:  {item["InterfaceType"]}");
                        Console.WriteLine($"Статус:  {item["Status"]}"); ;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Не удалось получить информацию о диске" + ex.Message);
            }
        }
    }
}
