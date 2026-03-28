using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationSeti
{
    public class SetiSettings
    {
        public void Setiinformation()
        {
            try
            {
                using var information = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
                {
                    foreach (var item in information.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\nНазвание адаптера:  {item["Name"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Включено:  {item["NetEnabled"]}");
                        Console.WriteLine($"Физический адаптер:  {item["PhysicalAdapter"]}");
                        Console.WriteLine($"Cкорость:  {item["Speed"]}");
                        Console.WriteLine($"Производитель:  {item["Manufacturer"]}");
                        Console.WriteLine($"Статус Соединения(смотрите справку ниже):  {item["NetConnectionStatus"]}");
                        Console.WriteLine();
                        Console.WriteLine("0 = Disconnected\r\n\r\n1 = Connecting\r\n\r\n2 = Connected\r\n\r\n3 = Disconnecting\r\n\r\n4 = Hardware not present\r\n\r\n5 = Hardware disabled\r\n\r\n6 = Hardware malfunction\r\n\r\n7 = Media disconnected\r\n\r\n8 = Authenticating\r\n\r\n9 = Authentication succeeded\r\n\r\n10 = Authentication failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить информацию о настройках сети" + ex.Message);
            }
        }
    }
}
