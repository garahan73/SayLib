using Say32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Say32
{
    public class FileUtil
    {
        public static void EnsureFolderPath( string folder )
        {
            var fullPath = Path.GetFullPath(folder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

        }

        public static void CleanupFolderContents( string folderPath )
        {
            if (!Directory.Exists(folderPath))
                return;

            foreach (var path in Directory.GetFiles(folderPath))
            {
                CodeUtil.SafeRun(()=>File.Delete(path));

            }
        }

        public static void CleanOldFiles( string folderPath, int fileCount, string? fileSearchPattern = null )
        {
            if (!Directory.Exists(folderPath))
                return;

            var files = fileSearchPattern == null ? Directory.GetFiles(folderPath) :
                Directory.GetFiles(folderPath, fileSearchPattern);

            if (files.Count() <= fileCount) return;

            foreach (var file in files.OrderBy(f => File.GetLastWriteTime(f)).Take(files.Count() - fileCount))
            {
                Run.Safely(() =>
                {
                    if (File.Exists(file))
                        File.Delete(file);
                });
            }
        }

        public static int GetFileCount( string folder ) => Directory.GetFiles(folder).Count();

        public static int GetSubfolderCount( string folder ) => Directory.GetDirectories(folder).Count();

        public static int GetFileAndSubfolderCount( string folder ) => GetFileCount(folder) + GetSubfolderCount(folder);

        public static string? GetLastFilePath(string folderPath, string? searchPattern = null)
        {
            var dir = new DirectoryInfo(folderPath);
            var files = searchPattern == null ? dir.GetFiles() : dir.GetFiles(searchPattern);

            return files.OrderByDescending(f => f.LastWriteTime).FirstOrDefault()?.FullName;

        }
    }
}
