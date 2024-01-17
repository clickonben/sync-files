using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncFiles.Utilities
{
    internal class FileSystemWatcherManager : IDisposable
    {
        private readonly IEnumerable<FileSystemWatcher> _watchers;
        private readonly DirectoryInfo _destination;

        public FileSystemWatcherManager(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters)
        {
            _watchers = filters.Select(f => CreateWatcher(source.FullName, f)).ToArray();
            _destination = destination;
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        private FileSystemWatcher CreateWatcher(string path, string filter)
        {
            var watcher = new FileSystemWatcher(path, filter)
            {
                IncludeSubdirectories = true
            };
            watcher.Changed += OnCreateOrChange;
            watcher.Created += OnCreateOrChange;
            watcher.Deleted += OnDelete;
            watcher.Renamed += OnRename;
            return watcher;
        }

        private void OnCreateOrChange(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File created or changed: {e.FullPath}");

            FileInfo dest = new (Path.Join(_destination?.FullName, e.Name));

            Console.WriteLine($"Copying {e.FullPath} to {dest}");

            try
            {
                if(dest.Directory != null && !dest.Directory.Exists)
                {
                    dest.Directory.Create();
                }
                File.Copy(e.FullPath, dest.FullName, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void OnDelete(object Sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File deleted: {e.FullPath}");
            var dest = Path.Join(_destination?.FullName, e.Name);

            Console.WriteLine($"Deleting {dest}");

            try
            {
                File.Delete(dest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void OnRename(object Sender, RenamedEventArgs e)
        {
            Console.WriteLine($"File {e.OldFullPath} renamed to {e.FullPath}");

            var oldfile = Path.Join(_destination?.FullName, e.OldName);
            var newfile = Path.Join(_destination?.FullName, e.Name);

            Console.WriteLine($"Renaming {oldfile} to {newfile}");

            try
            {
                File.Move(oldfile, newfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void Dispose()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }
    }
}
