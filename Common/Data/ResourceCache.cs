using System.Collections;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Text;
using EggLink.DanhengServer.Data.Config.Scene;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EggLink.DanhengServer.Data;

public static class CompressionHelper
{
    public static byte[] Compress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0) return [];

        try
        {
            if (data.Length < 1024)
            {
                var result = new byte[data.Length + 1];
                result[0] = 0;
                Buffer.BlockCopy(data, 0, result, 1, data.Length);
                return result;
            }

            using var output = new MemoryStream();
            output.WriteByte(1);
            using (var compressor = new DeflateStream(output, CompressionMode.Compress, true))
            {
                compressor.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }
        catch
        {
            var result = new byte[data.Length + 1];
            result[0] = 0;
            Buffer.BlockCopy(data, 0, result, 1, data.Length);
            return result;
        }
    }

    public static byte[] Decompress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0) return [];

        try
        {
            if (data[0] == 0)
            {
                var result = new byte[data.Length - 1];
                Buffer.BlockCopy(data, 1, result, 0, result.Length);
                return result;
            }

            using var input = new MemoryStream(data, 1, data.Length - 1);
            using var decompressor = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            decompressor.CopyTo(output);
            return output.ToArray();
        }
        catch
        {
            return data;
        }
    }
}

public class ResourceCacheData
{
    public Dictionary<string, byte[]> GameDataValues { get; set; } = [];
}

public class IgnoreJsonIgnoreContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        property.Ignored = false;
        return property;
    }
}

public class ResourceCache
{
    public static readonly JsonSerializerSettings Serializer = new()
    {
        ContractResolver = new IgnoreJsonIgnoreContractResolver(),
        TypeNameHandling = TypeNameHandling.Auto,
        Converters =
        {
            new ConcurrentBagConverter<PropInfo>(),
            new ConcurrentDictionaryConverter<string, FloorInfo>()
        }
    };

    public static Logger Logger { get; } = new("ResourceCache");
    public static string CachePath { get; } = ConfigManager.Config.Path.ConfigPath + "/Resource.cache";
    public static bool IsComplete { get; set; } = true; // Custom in errors to ignore some error

    public static Task SaveCache()
    {
        return Task.Run(() =>
        {
            var cacheData = new ResourceCacheData
            {
                GameDataValues = typeof(GameData)
                    .GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(p => p.GetValue(null) != null)
                    .ToDictionary(
                        p => p.Name,
                        p => CompressionHelper.Compress(
                            Encoding.UTF8.GetBytes(
                                JsonConvert.SerializeObject(p.GetValue(null), Serializer)
                            )
                        )
                    )
            };

            File.WriteAllText(CachePath, JsonConvert.SerializeObject(cacheData));
            Logger.Info(I18NManager.Translate("Server.ServerInfo.GeneratedItem",
                I18NManager.Translate("Word.Cache")));
        });
    }

    public static bool LoadCache()
    {
        var buffer = new byte[new FileInfo(CachePath).Length];
        var viewAccessor = MemoryMappedFile.CreateFromFile(CachePath, FileMode.Open).CreateViewAccessor();
        viewAccessor.ReadArray(0, buffer, 0, buffer.Length);

        var cacheData = JsonConvert.DeserializeObject<ResourceCacheData>(Encoding.UTF8.GetString(buffer));
        if (cacheData == null) return false;

        Parallel.ForEach(
            typeof(GameData).GetProperties(BindingFlags.Public | BindingFlags.Static),
            prop =>
            {
                if (!cacheData.GameDataValues.TryGetValue(prop.Name, out var valueBytes)) return;

                try
                {
                    var json = Encoding.UTF8.GetString(CompressionHelper.Decompress(valueBytes));
                    var deserialized = JsonConvert.DeserializeObject(json, prop.PropertyType, Serializer);
                    if (deserialized == null) return;

                    if (prop.SetMethod != null)
                    {
                        prop.SetValue(null, deserialized);
                        return;
                    }

                    var current = prop.GetValue(null);
                    if (current == null) return;

                    if (TryPopulateNonGenericCollections(current, deserialized)) return;
                    if (TryPopulateGenericDictionary(current, deserialized)) return;
                    if (TryPopulateGenericListLike(current, deserialized)) return;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to load cache property \"{prop.Name}\"", ex);
                }
            }
        );

        Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadedItem",
            I18NManager.Translate("Word.Cache")));

        return true;
    }

    private static bool TryPopulateNonGenericCollections(object target, object source)
    {
        if (target is IDictionary targetDict && source is IDictionary sourceDict)
        {
            targetDict.Clear();
            foreach (DictionaryEntry entry in sourceDict) targetDict[entry.Key] = entry.Value;
            return true;
        }

        if (target is IList targetList && source is IList sourceList)
        {
            targetList.Clear();
            foreach (var item in sourceList) targetList.Add(item);
            return true;
        }

        return false;
    }

    private static bool TryPopulateGenericDictionary(object target, object source)
    {
        var targetType = target.GetType();
        var sourceType = source.GetType();

        // Quick exit: must be enumerable of KVP and have Clear().
        var clear = targetType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
        if (clear == null) return false;
        if (!typeof(IEnumerable).IsAssignableFrom(sourceType)) return false;

        // TryAdd(TKey,TValue) preferred (works for ConcurrentDictionary)
        var tryAdd = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "TryAdd" && m.GetParameters().Length == 2);

        // Add(TKey,TValue) fallback
        var add = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 2);

        if (tryAdd == null && add == null) return false;

        clear.Invoke(target, null);

        foreach (var kvp in (IEnumerable)source)
        {
            var kvpType = kvp.GetType();
            var keyProp = kvpType.GetProperty("Key");
            var valueProp = kvpType.GetProperty("Value");
            if (keyProp == null || valueProp == null) continue;

            var key = keyProp.GetValue(kvp);
            var value = valueProp.GetValue(kvp);
            if (key == null) continue;

            if (tryAdd != null)
            {
                tryAdd.Invoke(target, [key, value]);
            }
            else
            {
                add!.Invoke(target, [key, value]);
            }
        }

        return true;
    }

    private static bool TryPopulateGenericListLike(object target, object source)
    {
        var targetType = target.GetType();
        var sourceType = source.GetType();
        if (!typeof(IEnumerable).IsAssignableFrom(sourceType)) return false;

        var clear = targetType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
        if (clear == null) return false;

        var add = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
        if (add == null) return false;

        clear.Invoke(target, null);
        foreach (var item in (IEnumerable)source) add.Invoke(target, [item]);
        return true;
    }

    public static void ClearGameData()
    {
        var properties = typeof(GameData).GetProperties(BindingFlags.Public | BindingFlags.Static);

        foreach (var prop in properties)
        {
            var propType = prop.PropertyType;
            var emptyValue = propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                ? Activator.CreateInstance(propType)
                : propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>)
                    ? Activator.CreateInstance(propType)
                    : propType.IsClass
                        ? Activator.CreateInstance(propType)
                        : null;

            prop.SetValue(null, emptyValue);
        }
    }
}
