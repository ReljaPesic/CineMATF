using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Cinema.API.Entities;

namespace Cinema.API.Converters;

public partial class CityEnumConverter : JsonConverter<City>
{
    public override City Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string value for City enum");

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            throw new JsonException("City value cannot be null or empty");

        // Convert "Novi Sad" back to "NoviSad" for enum parsing
        var enumValue = value.Replace(" ", "");

        if (Enum.TryParse<City>(enumValue, true, out var result))
            return result;

        throw new JsonException($"Invalid City value: {value}. Valid values are: Beograd, Novi Sad, Nis, Kragujevac");
    }

    public override void Write(Utf8JsonWriter writer, City value, JsonSerializerOptions options)
    {
        var enumName = value.ToString();

        // Add space before capital letters for "Novi Sad" format
        var formatted = NameRegex().Replace(enumName, "$1 $2");

        writer.WriteStringValue(formatted);
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex NameRegex();
}
