using DirectoryStatistic.InformationPC;
using DirectoryStatistic.InformationSeti;
using DirectoryStatistic.temperaturePC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class DirectoryPath
    {
        public async Task<string> Path()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Введите директорию для сканирования");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("F1 - Информацию о вашем соединение");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("F2 - Информация о пк и его компонентов");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("F3 - информация о температуре с датчиков");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("F4 - топ процессов по нагрузке на цп и памяти");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Escape для отмены");
            Console.BackgroundColor = ConsoleColor.Black;
            var inputBuilder = new StringBuilder();
             
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("\nОтмена ввода пути");
                        return null;
                    }

                    else if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        string path = inputBuilder.ToString();

                        if (string.IsNullOrWhiteSpace(path))
                        {
                            Console.WriteLine("Путь не может быть пустым. Введите снова:");
                            inputBuilder.Clear();
                            continue;
                        }

                        Console.WriteLine($"Путь: {path}");
                        return path;
                    }

                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (inputBuilder.Length > 0)
                        {
                            inputBuilder.Remove(inputBuilder.Length - 1, 1);
                            Console.Write("\b \b");
                        }
                    }
                    else if (key.Key == ConsoleKey.F1)
                    {
                        HTTPZapr zapros = new HTTPZapr();
                        Console.WriteLine("Информация о вашем соединении:");
                        Console.WriteLine();
                        await zapros.Httpzapros();
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        inputBuilder.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                    else if (key.Key == ConsoleKey.F2)
                    {
                        Console.WriteLine();
                        GPUInformation information = new GPUInformation();
                        information.GPUiNFO();

                        CPUInformation information1 = new CPUInformation();
                        information1.CPUINFORM();

                        RAMInformation information2 = new RAMInformation();
                        information2.RAMINFORMATIO();

                        DirectoryStatistic.InformationPC.DriveInfo information3 = new DirectoryStatistic.InformationPC.DriveInfo();
                        information3.DRIVEIBFO();

                        MotherBoardInfo information4 = new MotherBoardInfo();
                        information4.MotherBoardInfos();

                        OC information5 = new OC();
                        information5.OCINFROMATION();

                        SetiSettings information6 = new SetiSettings();
                        information6.Setiinformation();
                    }

                    else if (key.Key == ConsoleKey.F3)
                    {
                        TemperatureConsoleWrite.PrintTemperatures();
                    }
                    else if (key.Key == ConsoleKey.F4)
                    { 
                        TopProcess top = new TopProcess();
                        top.ShowTopProcesses();
                    }
                }

                Thread.Sleep(50);
            }
        }
    }
}
