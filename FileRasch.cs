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
        public List<FileInfo> SearchFile { get; } = new List<FileInfo>();
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

        public void SearchFIleInDirectory(FileInfo file, string Name, string rach)
        {
            SearchFile.Clear();

            var allFiles = new List<FileInfo>();
            allFiles.AddRange(Filejpg);
            allFiles.AddRange(Filepng);
            allFiles.AddRange(FileTxt);
            allFiles.AddRange(FilePdf);
            allFiles.AddRange(FilesDoc);
            allFiles.AddRange(FilesOther);

            Console.WriteLine($"Ищу файл: {Name} с расширением: {rach}");
            Console.WriteLine($"Всего файлов в хранилище: {allFiles.Count}");

            foreach (var currentFile in allFiles)
            {
                var fileName = Path.GetFileName(currentFile.Name);
                var fileExtension = Path.GetExtension(currentFile.Name).ToLower();

                if ((Name == fileName || string.IsNullOrEmpty(Name)) &&
                    (rach == fileExtension || string.IsNullOrEmpty(rach)))
                {
                    SearchFile.Add(currentFile); 
                    Console.WriteLine($"✓ Найден: {currentFile.FullName}"); 
                }
            }

            Console.WriteLine($"Найдено совпадений: {SearchFile.Count}");
        }
        
        public void PrintSearch()
        {
            Console.WriteLine("=== Найдено совпадений ===" + SearchFile.Count);
            if (SearchFile != null)
            { 
               Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($": {SearchFile}");
                Console.ForegroundColor= ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Совпадений не найдено");
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
