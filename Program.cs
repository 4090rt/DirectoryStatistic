// See https://aka.ms/new-console-template for more information
using DirectoryStatistic;
DirectoryPath PATH = new DirectoryPath();
ReadDirectory directory = new ReadDirectory();
SimpleFileStorage directory2 = new SimpleFileStorage();
SimpleFileStorage simole = new SimpleFileStorage();
string path = PATH.Path();
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

                    var searchResults = directory.SortFilesSimplySearch(allFiles,name,extension);
                    searchResults.PrintSearch();

                    Console.WriteLine("F1 - Новый поиск | ESC - Выход | Enter - Повторное сканирование");
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


