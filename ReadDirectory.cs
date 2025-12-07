using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class ReadDirectory
    {
        public Task<List<FileInfo>> ReadDirectoryy(string directoryPath)
        {
            var allfiles = new ConcurrentBag<FileInfo>();
            var directories = new Stack<string>();
            directories.Push(directoryPath);
            try
            {
                while (directories.Count > 0)
                {
                    var current = directories.Pop();
                    string[] files = Directory.GetFiles(current);
                    Parallel.ForEach(files, file =>
                    {
                        allfiles.Add(new FileInfo(file));
                    });

                    string[] poddirectory = Directory.GetDirectories(current);
                    foreach (var direct in poddirectory)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Нет доступа к папке" + ex.Message);
            }
            return Task.FromResult(allfiles.ToList());
        }


        public SimpleFileStorage SortFilesSimply(List<FileInfo> allFiles)
        {
            var exte = new SimpleFileStorage();

            Parallel.ForEach(allFiles, file =>
            {
                lock (exte)
                {
                    exte.Addfile(file);
                }
            });
            return exte;
        }

        public SimpleFileStorage SortFilesSimplySearch(List<FileInfo> allFiles, string name, string rach)
        {
            var exte = new SimpleFileStorage();

            Parallel.ForEach(allFiles, file =>
            {
                lock (exte)
                {
                    exte.SearchFIleInDirectory(file, name, rach);
                }
            });
            return exte;
        }
    }
}
