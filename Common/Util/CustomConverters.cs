using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Util;

public class ConcurrentBagConverter<T> : JsonConverter<ConcurrentBag<T>>
{
    public override void WriteJson(JsonWriter writer, ConcurrentBag<T>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        serializer.Serialize(writer, value.ToArray());
    }

    public override ConcurrentBag<T>? ReadJson(JsonReader reader, Type objectType, ConcurrentBag<T>? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var array = serializer.Deserialize<T[]>(reader);
        return array != null ? new ConcurrentBag<T>(array) : new ConcurrentBag<T>();
    }
}

public class ConcurrentDictionaryConverter<TKey, TValue> : JsonConverter<ConcurrentDictionary<TKey, TValue>>
    where TKey : notnull
{
    public override void WriteJson(JsonWriter writer, ConcurrentDictionary<TKey, TValue>? value,
        JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        serializer.Serialize(writer, value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    public override ConcurrentDictionary<TKey, TValue>? ReadJson(JsonReader reader, Type objectType,
        ConcurrentDictionary<TKey, TValue>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var dictionary = serializer.Deserialize<Dictionary<TKey, TValue>>(reader);
        return dictionary != null
            ? new ConcurrentDictionary<TKey, TValue>(dictionary)
            : new ConcurrentDictionary<TKey, TValue>();
    }
}