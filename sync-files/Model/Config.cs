using Newtonsoft.Json;

namespace SyncFiles.Model
{
    internal class Config
    {
        [JsonConstructor]
        public Config(DirectoryInfo source, DirectoryInfo destination, bool initialise, IEnumerable<string> filters, IEnumerable<string> filesToKeep)
        {
            Source = source;
            Destination = destination;
            Initialise = initialise;
            Filters = filters;
            FilesToKeep = filesToKeep;
        }

        [JsonConverter(typeof(DirectoryInfoJsonConverter))]
        public DirectoryInfo Source { get; }
        [JsonConverter(typeof(DirectoryInfoJsonConverter))]
        public DirectoryInfo Destination { get; }
        public bool Initialise { get; }
        public IEnumerable<string> Filters { get; set; }
        public IEnumerable<string>? FilesToKeep { get; set; }

    }
}
