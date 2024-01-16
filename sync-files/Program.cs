using System.CommandLine;
using System.CommandLine.Parsing;
using Newtonsoft.Json;
using SyncFiles.Model;
using SyncFiles.Utilities;

namespace SyncFiles;

class Program
{
    static DirectoryInfo? _destination;
    static async Task<int> Main(string[] args)
    {

        Config? configFile = await TryLoadConfigFromFileAsync("config.json");
        if (configFile != null)
        {
            WatchFolder(configFile);
            return 0;
        }

        RootCommand rootCommand = ParseArguments();       

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task<Config?> TryLoadConfigFromFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading config file: {ex.Message}");
            }
        }
        return null;
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

    static void WatchFolder(Config config)
    {
        if (config.Initialise)
        {
            Initialiser.Initialise(config);
        }

        try
        {
            Console.WriteLine($"Source: {config.Source}");
            Console.WriteLine($"Destination: {config.Destination}");
            Console.WriteLine($"Filters: {string.Join(" ", config.Filters)}");
            _destination = config.Destination;
            using var watcherManager = new FileSystemWatcherManager(config.Source, config.Destination, config.Filters);
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void WatchFolder(DirectoryInfo source, DirectoryInfo destination, IEnumerable<string> filters, bool initialise, IEnumerable<string>? filesToKeep)
    {
        Config config = new(source, destination, initialise, filters, filesToKeep ?? Array.Empty<string>());

        if (!ConfigValid(config))
        {
            return;
        }
        
        WatchFolder(config);
    }

    static bool ConfigValid(Config config)
    {
        if (config.Source == null)
        {
            Console.WriteLine("Required parameter --source is missing or could not be parsed corectly.");
            DisplayHelpText();
            return false;
        }
        if (config.Destination == null)
        {
            Console.WriteLine("Required parameter --destination is missing or could not be parsed corectly.");
            DisplayHelpText();
            return false;
        }
        if (config.Filters == null || !config.Filters.Any())
        {
            Console.WriteLine("Required parameter --filters is missing or could not be parsed corectly.");
            DisplayHelpText();
            return false;
        }
        if (!config.Source.Exists)
        {
            Console.WriteLine($"Source folder '{config.Source.FullName}' does not exist.");
            return false;
        }
        if (!config.Destination.Exists)
        {
            Console.WriteLine($"Destination folder '{config.Destination.FullName}' does not exist.");
            return false;
        }

        if (!config.Initialise && config.FilesToKeep != null && config.FilesToKeep.Any())
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