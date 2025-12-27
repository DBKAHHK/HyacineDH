using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Util;

public sealed class LenientStringEnumConverter<TEnum> : JsonConverter where TEnum : struct, Enum
{
    private static readonly ConcurrentDictionary<string, byte> UnknownValues = new();
    private static readonly Logger Logger = new("Json");

    public override bool CanConvert(Type objectType)
    {
        var t = Nullable.GetUnderlyingType(objectType) ?? objectType;
        return t == typeof(TEnum);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (Nullable.GetUnderlyingType(objectType) != null) return null;
            return default(TEnum);
        }

        if (reader.TokenType == JsonToken.String)
        {
            var raw = (reader.Value as string) ?? string.Empty;
            if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed)) return parsed;

            if (UnknownValues.TryAdd(raw, 0))
                Logger.Warn($"Unknown enum value \"{raw}\" for {typeof(TEnum).FullName}, fallback to {default(TEnum)}");

            return default(TEnum);
        }

        if (reader.TokenType == JsonToken.Integer)
        {
            try
            {
                var value = Convert.ToInt64(reader.Value);
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }
            catch
            {
                return default(TEnum);
            }
        }

        return default(TEnum);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(value.ToString());
    }
}

