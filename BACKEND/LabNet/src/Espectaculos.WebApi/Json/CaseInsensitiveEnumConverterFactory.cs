using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Espectaculos.WebApi.Json;

public class CaseInsensitiveEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(CaseInsensitiveEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class CaseInsensitiveEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (s is null) throw new JsonException("Enum string was null");
            if (Enum.TryParse<T>(s, ignoreCase: true, out var val)) return val;
            throw new JsonException($"Unable to convert '{s}' to enum {typeof(T)}");
        }
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var i))
        {
            return (T)Enum.ToObject(typeof(T), i);
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
