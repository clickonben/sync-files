using System.Text.RegularExpressions;
using SyncFiles.Model;

namespace SyncFiles.Utilities
{
    internal static class Initialiser
    {
        internal static void Initialise(Config config)
        {
            Console.WriteLine($"Initialising {config.Destination.FullName}.");
            Console.WriteLine($"Deleting contents of {config.Destination.FullName}.");
            if (config.FilesToKeep != null && config.FilesToKeep.Any())
            {
                Console.WriteLine($"Excluding the following files {string.Join(',', config.FilesToKeep)}.");
            }
            DeleteFilesAndFolders(config.Destination, config.FilesToKeep ?? Array.Empty<string>());
            CopyFilesAndFolders(config.Source, config.Destination, config.Filters);
        }

        private static void DeleteFilesAndFolders(DirectoryInfo destination, IEnumerable<string> filesToKeep)
        {

            var files = destination.GetFiles();
            foreach (var file in files)
            {
                if (!filesToKeep.Any(f => f == file.Name))
                {
                    file.Delete();
                    Console.WriteLine($"Deleted file: {file.FullName}");
                }
            }

            var directories = destination.GetDirectories();
            foreach (var dir in directories)
            {
                dir.Delete(true);
                Console.WriteLine($"Deleted directory: {dir.FullName}");
            }
        }

        private static void CopyFilesAndFolders(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters)
        {            
            if (!destination.Exists)
            {
                Console.WriteLine($"Creating folder: {destination.FullName}");
                try
                {
                    destination.Create();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }                               
            }

            var files = source.GetFiles()
                  .Where(file => filters.Any(filter => IsMatch(filter, file.Name)));

            foreach (var file in files)
            {
                string destFile = Path.Combine(destination.FullName, file.Name);
                Console.WriteLine($"Copying file: {file.FullName} to {destFile}");
                try
                {
                    file.CopyTo(destFile, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }                             
            }

            var directories = source.GetDirectories()
                .Where(dir => dir.GetFiles()
                    .Any(file => filters
                        .Any(filter => IsMatch(filter, file.Name))));
            foreach (var dir in directories)
            {
                string destDir = Path.Combine(destination.FullName, dir.Name);
                DirectoryInfo newDestination = new(destDir);

                CopyFilesAndFolders(dir, newDestination, filters);
            }
        }

        private static bool IsMatch(string pattern, string input)
        {
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
        }

    }


}
