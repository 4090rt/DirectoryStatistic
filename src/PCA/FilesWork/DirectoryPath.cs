using DirectoryStatistic.Http.HttpRequest;
using DirectoryStatistic.Http.JitterClass;
using DirectoryStatistic.Http.Parser;
using DirectoryStatistic.InformationPC;
using DirectoryStatistic.InformationSeti;
using DirectoryStatistic.temperaturePC;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.FilesWork
{
    public class DirectoryPath
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly Jitter _jitter;

        public DirectoryPath(IServiceProvider serviceProvider, ILogger logger, IMemoryCache memoryCache, Jitter jitter)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _memoryCache = memoryCache;
            _jitter = jitter;

        }

        public async Task<string> Path()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Введите директорию для сканирования");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("F1 - Информацию о вашем соединение");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("F2 - Информация о пк и его комплектующих");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("F3 - информация о температуре с датчиков");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("F4 - топ процессов по нагрузке на цп и память");
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
                        var logger = _serviceProvider.GetRequiredService<ILogger<HTTPZapr>>();
                        var memoryCache = _serviceProvider.GetRequiredService<IMemoryCache>();
                        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
                        var parseLogger = _serviceProvider.GetRequiredService<ILogger<Parsing>>();
                        var parse = new Parsing(parseLogger);
                        var semaphore = new SemaphoreSlim(1, 1);
                        
                        HTTPZapr zapros = new HTTPZapr(logger, memoryCache, parse, httpClientFactory, semaphore);
                        Console.WriteLine("Информация о вашем соединении:");
                        Console.WriteLine();
                        var result = await zapros.Request();
                        var first = result.FirstOrDefault();
                        if (first != null)
                        {
                            Console.WriteLine($"IP: {first.IP}");
                            Console.WriteLine($"Город: {first.City}");
                            Console.WriteLine($"Провайдер: {first.Org}");
                            Console.WriteLine($"Регион: {first.Region}");
                            Console.WriteLine($"Страна: {first.Country}");
                            Console.WriteLine($"Почтовый индекс: {first.Postal}");
     
                        }
                        Console.WriteLine("=== ЗАМЕР ПИНГА И ОТКЛОНЕНИЯ ===\n");

                        Console.WriteLine("Замер пинга до Google (5 запросов)...");
                        var jitterResult = await _jitter.JitterSc("https://www.google.com", 5);

                        Console.WriteLine($"\nКоличество замеров: {jitterResult.Count}");
                        Console.WriteLine($"Макс. пинг: {jitterResult.MaxMs} мс");
                        Console.WriteLine($"Мин. пинг: {jitterResult.MinMS} мс");
                        Console.WriteLine($"Средний пинг: {jitterResult.Average:F2} мс");
                        Console.WriteLine($"Jitter (отклонение): {jitterResult.JitterMs:F2} мс");
                        Console.WriteLine($"Общее время замера: {jitterResult.Timer} мс");
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

                        InformationPC.DriveInfo information3 = new InformationPC.DriveInfo();
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
                        var topProcess = _serviceProvider.GetRequiredService<TopProcess>();
                        await topProcess.ShowTopProcessesAsync();
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
