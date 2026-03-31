// See https://aka.ms/new-console-template for more information
using DirectoryStatistic.FilesWork;
using DirectoryStatistic.Http.HttpRequest;
using DirectoryStatistic.Http.JitterClass;
using DirectoryStatistic.InformationPC;
using DirectoryStatistic.InformationSeti;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net;

class Program
{
    private readonly ILogger _logger;

    public Program(ILogger logger)
    {
        _logger = logger;
    }


    static async Task Main(string[] asrgs)
    {
        var service = new ServiceCollection();

        service.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        service.AddMemoryCache();
        service.AddHttpClient("ClientProvider", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(30),
            Polly.Timeout.TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task) =>
            {
                Console.WriteLine($"⏰ Request timed out after {timespan}");
                return Task.CompletedTask;
            })).AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcomem, timespan) =>
                {
                    Console.WriteLine($"🔌 Circuit opened for {timespan}");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("⚠️ Circuit half-open");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit reset");
                })).AddTransientHttpErrorPolicy(polly => polly.WaitAndRetryAsync(3, retrycount =>
                TimeSpan.FromSeconds(Math.Pow(2, retrycount))
                + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetry: (outcome, timespan, retrycount, context) =>
                {
                    Console.WriteLine($"🔄 Retry {retrycount} after {timespan}");
                })).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                {
                    EnableMultipleHttp2Connections = true,

                    PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(10),

                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

                    MaxConnectionsPerServer = 10,
                    UseCookies = false,
                    AllowAutoRedirect = false
                });

        service.AddScoped<HTTPZapr>();
        service.AddScoped<HttpRequestPing>();
        service.AddScoped<Jitter>();
        service.AddSingleton<TopProcess>();

        var serviceProvider = service.BuildServiceProvider();
        var jitter = serviceProvider.GetRequiredService<Jitter>();

        var logger = serviceProvider.GetRequiredService<ILogger<DirectoryPath>>();
        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        DirectoryPath PATH = new DirectoryPath(serviceProvider, logger, memoryCache, jitter);
        ReadDirectory directory = new ReadDirectory();
        SimpleFileStorage directory2 = new SimpleFileStorage();
        SimpleFileStorage simole = new SimpleFileStorage();
        string path = await PATH.Path();
        // ДИАГНОСТИКА: что возвращает метод Path()?
        Console.WriteLine($"=== ДЕБАГ ===");
        Console.WriteLine($"path == null: {path == null}");
        Console.WriteLine($"path value: '{path ?? "NULL"}'");
        Console.WriteLine($"path type: {path?.GetType().Name ?? "NULL"}");
        Console.WriteLine($"=============");

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("ОШИБКА: путь не может быть пустым!");
            Console.WriteLine("Проверьте метод PATH.Path()");
            return;
        }
        Console.WriteLine($"Вы указали {path}  - Хотите продолжить?");
        Console.WriteLine("Чтобы продолжить нажмите");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(" Enter");
        Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" или же нажмите");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" ESCAPE");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" для отмены");
            Console.WriteLine();
            ConsoleKeyInfo keyy;
            do
            {
                keyy = Console.ReadKey(intercept: true);

                if (keyy.Key == ConsoleKey.Enter)
                {
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        Console.WriteLine($"Сканирую папку: {path}");

                        var allFiles = await directory.ReadDirectoryy(path);
                        var storage = directory.SortFilesSimply(allFiles);
                        storage.PrintSimpleStats();

                        Console.WriteLine("=== ПОИСК ФАЙЛОВ ===");
                        Console.WriteLine("F1 - Поиск файла");
                        Console.WriteLine("F2 - Информация о ПК");
                        Console.WriteLine("F3 - Информация о сети");
                        Console.WriteLine("F4 - Замер пинга и отклонения");
                        Console.WriteLine("F5 - Топ процессов");
                        Console.WriteLine("ESC - Выйти из поиска");
                        Console.WriteLine("Enter - Повторить сканирование");

                        bool inSearchMode = true;

                        while (inSearchMode)
                        {
                            var searchKey = Console.ReadKey(intercept: true);

                            if (searchKey.Key == ConsoleKey.F1)
                            {
                                Console.Write("\nВведите имя файла (или Enter для любого): ");
                                string name = Console.ReadLine();

                                Console.Write("Введите расширение (например .txt, или Enter для любого): ");
                                string extension = Console.ReadLine();

                                var searchResults = directory.SortFilesSimplySearch(allFiles, name, extension);
                                searchResults.PrintSearch();

                                Console.WriteLine("\nF1 - Новый поиск | F2 - Информация о ПК | F3 - Информация о сети | F4 - Замер пинга | F5 - Топ процессов | ESC - Выход | Enter - Повторное сканирование");
                            }
                            else if (searchKey.Key == ConsoleKey.F2)
                            {
                                Console.Clear();
                                Console.WriteLine("=== ИНФОРМАЦИЯ О ПК ===\n");
                                
                                var cpuInfo = new CPUInformation();
                                cpuInfo.CPUINFORM();
                                
                                var gpuInfo = new GPUInformation();
                                gpuInfo.GPUiNFO();
                                
                                var ramInfo = new RAMInformation();
                                ramInfo.RAMINFORMATIO();
                                
                                var mbInfo = new MotherBoardInfo();
                                mbInfo.MotherBoardInfos();
                                
                                var driveInfoPC = new DirectoryStatistic.InformationPC.DriveInfo();
                                driveInfoPC.DRIVEIBFO();

                                Console.WriteLine("\n=== НАЖМИТЕ ЛЮБУЮ КЛАВИШУ ДЛЯ ВОЗВРАТА ===");
                                Console.ReadKey(intercept: true);
                            }
                            else if (searchKey.Key == ConsoleKey.F1)
                            {
                                Console.Clear();
                                Console.WriteLine("=== ИНФОРМАЦИЯ О СЕТИ ===\n");
                                
                                var ocInfo = new OC();
                                ocInfo.OCINFROMATION();
                                
                                var setiInfo = new SetiSettings();
                                setiInfo.Setiinformation();

                                Console.WriteLine("=== ЗАМЕР ПИНГА И ОТКЛОНЕНИЯ ===\n");

                                Console.WriteLine("Замер пинга до Google (5 запросов)...");
                                var jitterResult = await jitter.JitterSc("https://www.google.com", 5);

                                Console.WriteLine($"\nКоличество замеров: {jitterResult.Count}");
                                Console.WriteLine($"Макс. пинг: {jitterResult.MaxMs} мс");
                                Console.WriteLine($"Мин. пинг: {jitterResult.MinMS} мс");
                                Console.WriteLine($"Средний пинг: {jitterResult.Average:F2} мс");
                                Console.WriteLine($"Jitter (отклонение): {jitterResult.JitterMs:F2} мс");
                                Console.WriteLine($"Общее время замера: {jitterResult.Timer} мс");

                            Console.WriteLine("\n=== НАЖМИТЕ ЛЮБУЮ КЛАВИШУ ДЛЯ ВОЗВРАТА ===");
                                Console.ReadKey(intercept: true);
                            }
                            else if (searchKey.Key == ConsoleKey.F5)
                            {
                                Console.Clear();
                                var topProcess = serviceProvider.GetRequiredService<TopProcess>();
                                await topProcess.ShowTopProcessesAsync(10, CancellationToken.None);

                                Console.WriteLine("\n=== НАЖМИТЕ ЛЮБУЮ КЛАВИШУ ДЛЯ ВОЗВРАТА ===");
                                Console.ReadKey(intercept: true);
                            }
                            else if (searchKey.Key == ConsoleKey.Escape)
                            {
                                Console.WriteLine("\nВыход из режима поиска");
                                inSearchMode = false;
                            }
                            else if (searchKey.Key == ConsoleKey.Enter)
                            {
                                Console.WriteLine("\nПовторное сканирование...");
                                inSearchMode = false;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Путь не найден или папка не существует");
                    }
                }
                else if (keyy.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nВыход из программы");
                    break;
                }

            } while (true);
    }
}


