using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync_files.Utilities
{
    internal static class Initialiser
    {
        internal static void Initialise(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filesToKeep)
        {
            Console.WriteLine($"Initialising {destination.FullName}.");
            Console.WriteLine($"Deleting contents of {destination.FullName}.");
            if (filesToKeep.Any())
            {
                Console.WriteLine($"Excluding the following files {string.Join(',', filesToKeep)}.");
            }
            DeleteFilesAndFolders(destination, filesToKeep ?? Array.Empty<string>());
            CopyFilesAndFolders(source, destination);
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

        private static void CopyFilesAndFolders(DirectoryInfo source, DirectoryInfo destination)
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

            var files = source.GetFiles();
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

            var directories = source.GetDirectories();
            foreach (var dir in directories)
            {
                string destDir = Path.Combine(destination.FullName, dir.Name);
                DirectoryInfo newDestination = new(destDir);

                CopyFilesAndFolders(dir, newDestination);
            }
        }

    }


}
