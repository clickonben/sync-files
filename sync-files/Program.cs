using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace SyncFiles;

class Program
{
    static DirectoryInfo? _destination;
    static async Task<int> Main(string[] args)
    {
        RootCommand rootCommand = ParseArguments();

        return await rootCommand.InvokeAsync(args);
    }

    private static RootCommand ParseArguments()
    {
        var rootCommand = new RootCommand("File sync app");
        var sourceOption = new Option<DirectoryInfo>("--source", "Source folder");
        var destinationOption = new Option<DirectoryInfo>("--destination", "Destination folder");
        var filtersOption = new Option<IEnumerable<string>>("--filters", "List of file extensions to monitor") { AllowMultipleArgumentsPerToken = true };
        rootCommand.AddOption(sourceOption);
        rootCommand.AddOption(destinationOption);
        rootCommand.AddOption(filtersOption);

        rootCommand.SetHandler(
            WatchFolder,
            sourceOption,
            destinationOption,
            filtersOption
        );
        return rootCommand;
    }

    static void WatchFolder(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters)
    {
        try
        {
            if(!ParametersValid(source, destination, filters))
            {
                return;
            };

            Console.WriteLine($"Source: {source}");
            Console.WriteLine($"Destination: {destination}");
            Console.WriteLine($"Filters: {string.Join(" ", filters)}");
            _destination = destination;

            var watchers = filters.Select(filter => new FileSystemWatcher(source.FullName, filter)
            {
                IncludeSubdirectories = true               
            });

            foreach( var watcher in watchers)
            {
                watcher.Changed += OnCreateOrChange;                
                watcher.Created += OnCreateOrChange;                
            }

            using var watcherManager = new FileSystemWatcherManager(watchers);
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static bool ParametersValid(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters)
    {        
        if (source == null)
        {
            Console.WriteLine("Required parameter --source is missing or could not be parsed corectly.");
            DisplayHelpText();
            return false;
        }
        if (destination == null)
        {
            Console.WriteLine("Required parameter --destination is missing or could not be parsed corectly.");
            DisplayHelpText();
            return false;
        }
        if (filters == null || !filters.Any())
        {
            Console.WriteLine("Required parameter --filters is missing or could not be parsed corectly.");
            DisplayHelpText();
            return false;
        }
        if (!source.Exists)
        {
            Console.WriteLine($"Source folder '{source.FullName}' does not exist.");
            return false;
        }
        if (!destination.Exists)
        {
            Console.WriteLine($"Destination folder '{destination.FullName}' does not exist.");
            return false;
        }
        return true;
    }

    static void DisplayHelpText()
    {
        Console.WriteLine("Example usage: sync-files --source \"c:\\users\\dave\\repos\" --destination \"d:\" --filters \"*.py\" \"*.mpy\"");
    }

    static void OnCreateOrChange(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"File created or changed: {e.FullPath}");

        var dest = Path.Join(_destination?.FullName, e.Name);

        Console.WriteLine($"Copying {e.FullPath} to {dest}");

        try
        {
            File.Copy(e.FullPath, dest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }   
    }

    static void OnDelete( object Sender, FileSystemEventArgs e)
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

    static void OnRename(object Sender, RenamedEventArgs e)
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

    public class FileSystemWatcherManager : IDisposable
    {
        private readonly IEnumerable<FileSystemWatcher> _watchers;

        public FileSystemWatcherManager(IEnumerable<FileSystemWatcher> watchers)
        {
            _watchers = watchers;
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = true;
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