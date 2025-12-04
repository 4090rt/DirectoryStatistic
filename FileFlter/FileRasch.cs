using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class SimpleFileStorage
    {
        public List<FileInfo> Filejpg { get; } = new List<FileInfo>();
        public List<FileInfo> Filepng { get; } = new List<FileInfo>();
        public List<FileInfo> FileTxt { get; } = new List<FileInfo>();
        public List<FileInfo> FilePdf { get; } = new List<FileInfo>();
        public List<FileInfo> FilesDoc { get; } = new List<FileInfo>();
        public List<FileInfo> FilesOther { get;} = new List<FileInfo>();


        public void Addfile(FileInfo file)
        {
            var name = Path.GetExtension(file.Name).ToLower();
            if (name == ".jpg" || name == ".jpeg")
            {
                Filejpg.Add(file);
            }
            else if (name == ".png")
            {
                Filepng.Add(file);
            }
            else if (name == ".txt")
            {
                FileTxt.Add(file);
            }
            else if (name == ".pdf")
            {
                FilePdf.Add(file);
            }
            else if (name == ".doc" || name == ".docx")
            {
                FilesDoc.Add(file);
            }
            else
            {
                FilesOther.Add(file);
            }
        }


        public void PrintSimpleStats()
        {
            Console.WriteLine("=== ПРОСТАЯ СТАТИСТИКА ===");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"JPG файлов: {Filejpg.Count}");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"PNG файлов: {Filepng.Count}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"TXT файлов: {FileTxt.Count}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"PDF файлов: {FilePdf.Count}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"DOC файлов: {FilesDoc.Count}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Других файлов: {FilesOther.Count}");
            Console.ForegroundColor = ConsoleColor.Gray;
            int total = Filejpg.Count + Filepng.Count + FileTxt.Count +
                      FilePdf.Count + FilesDoc.Count + FilesOther.Count;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nВсего файлов: {total}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
