using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.InformationSeti
{
    public class OC
    {
        public void OCINFROMATION()
        {
            try
            {
                using var information = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                {
                    foreach (var item in information.Get())
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\nОперационная система:  {item["Caption"]}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Архитектура:  {item["OSArchitecture"]}");
                        Console.WriteLine($"Версия:  {item["Version"]}");
                        Console.WriteLine($"Количество пользователей:  {item["NumberOfUsers"]}");
                        Console.WriteLine($"Зарегестрированный пользователь:  {item["RegisteredUser"]}");
                        Console.WriteLine($"Версия сборки:  {item["BuildNumber"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить информацию о ОС" + ex.Message);
            }
        }
    }
}
