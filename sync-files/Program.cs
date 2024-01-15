using System.CommandLine;
using System.CommandLine.Parsing;
using sync_files.Utilities;
using SyncFiles.Utilities;

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
        RootCommand rootCommand = new("File sync app");

        var sourceOption = new Option<DirectoryInfo>(
                aliases: ["--source", "-s"],
                description: "Source folder");

        var destinationOption = new Option<DirectoryInfo>(
                aliases: ["--destination", "-d"],
                description: "Destination folder");

        var filtersOption = new Option<IEnumerable<string>>(
                aliases: ["--filters", "-f"],
                description: "List of file extensions to monitor")
        { AllowMultipleArgumentsPerToken = true };

        var initialiseOption = new Option<bool>(
                aliases: ["--initialise", "-i"],
                description: "Whether or not to initialise the destination",
                getDefaultValue: () => true);

        var filesToKeepOption = new Option<IEnumerable<string>>(
                aliases: ["--keepFiles", "-kf"],
                description: "Files to exclude from initialisation")
                { AllowMultipleArgumentsPerToken = true }; ;


        rootCommand.AddOption(sourceOption);
        rootCommand.AddOption(destinationOption);
        rootCommand.AddOption(filtersOption);
        rootCommand.AddOption(initialiseOption);
        rootCommand.AddOption(filesToKeepOption);

        rootCommand.SetHandler(
            WatchFolder,
            sourceOption,
            destinationOption,
            filtersOption,
            initialiseOption,
            filesToKeepOption
        );

        return rootCommand;
    }

    static void WatchFolder(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters, bool initialise, IEnumerable<string>? filesToKeep)
    {
        if (!ParametersValid(source, destination, filters, initialise, filesToKeep))
        {
            return;
        };

        if(initialise)
        {
            Initialiser.Initialise(source, destination, filters, filesToKeep ?? Array.Empty<string>());
        }
        
        try
        {
            Console.WriteLine($"Source: {source}");
            Console.WriteLine($"Destination: {destination}");
            Console.WriteLine($"Filters: {string.Join(" ", filters)}");
            _destination = destination;
            using var watcherManager = new FileSystemWatcherManager(source, destination, filters);
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static bool ParametersValid(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters, bool initialise, IEnumerable<string>? filesToKeep)
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

        if (!initialise && filesToKeep != null && filesToKeep.Any()) 
        {
            Console.WriteLine("Parameter --keepFiles will be ignored because --initialise is false.");
        }
        return true;
    }

    static void DisplayHelpText()
    {
        Console.WriteLine("Example usage: sync-files --source \"c:\\source\" --destination \"c:\\destination\" --filters \"*.py\" \"*.mpy\" --initialise --keepFiles \"boot_out.txt\" \"settings.toml\"");
    }   
}