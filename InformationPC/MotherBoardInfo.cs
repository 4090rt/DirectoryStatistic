using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationPC
{
    public class MotherBoardInfo
    {
        public void MotherBoardInfos()
        {
            try
            {
                using var informatio = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                {
                    foreach (var item in informatio.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"\nМатеринская плата:  {item["Product"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Мануфактура:  {item["Manufacturer"]}");
                        Console.WriteLine($"Имя:  {item["Name"]}");
                        Console.WriteLine($"Статус:  {item["Status"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить информацию о материнской плате" + ex.Message);
            }
        }
    }
}
