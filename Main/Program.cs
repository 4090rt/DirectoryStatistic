// See https://aka.ms/new-console-template for more information
using DirectoryStatistic;
DirectoryPath PATH = new DirectoryPath();
ReadDirectory directory = new ReadDirectory();
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
        if (path != null)
        {
            var alldiles = await directory.ReadDirectoryy(path);
            var sort = directory.SortFilesSimply(alldiles);
            sort.PrintSimpleStats();
        }
        else
        {
            Console.WriteLine("Путь не найден");
        }
    }
}
while (true);
