using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class DirectoryPath
    {
        public string Path()
        {
            Console.WriteLine("Введите путь к директории которую хотите просканироваль или отмените операцию (Escape для отмены):");

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

                    else if (!char.IsControl(key.KeyChar))
                    {
                        inputBuilder.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }

                Thread.Sleep(50);
            }
        }
    }
}
