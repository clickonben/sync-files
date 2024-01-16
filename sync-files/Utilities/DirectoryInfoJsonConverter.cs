using Newtonsoft.Json;

internal class DirectoryInfoJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DirectoryInfo);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
            return null!;

        string path = (string)reader.Value;
        return new DirectoryInfo(path);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is DirectoryInfo directoryInfo)
        {
            writer.WriteValue(directoryInfo.FullName);
        }
        else
        {
            writer.WriteNull();
        }
    }
}
