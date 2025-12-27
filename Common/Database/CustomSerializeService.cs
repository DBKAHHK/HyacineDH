using EggLink.DanhengServer.Util;
using Newtonsoft.Json;
using SqlSugar;

namespace EggLink.DanhengServer.Database;

public class CustomSerializeService : ISerializeService
{
    private readonly JsonSerializerSettings _jsonSettings;

    public CustomSerializeService()
    {
        _jsonSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore, // ignore default values
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
    }

    public string SerializeObject(object value)
    {
        return JsonConvert.SerializeObject(value, _jsonSettings);
    }

    public T DeserializeObject<T>(string value)
    {
        try
        {
            var clazz = JsonConvert.DeserializeObject<T>(value)!;
            return clazz;
        }
        catch
        {
            // try to create empty instance
            try
            {
                Logger.GetByClassName().Warn("Error occured when load database, resetting the mistake value");
                var inst = Activator.CreateInstance<T>();
                return inst;
            }
            catch
            {
                return default!;
            }
        }
    }

    public string SugarSerializeObject(object value)
    {
        return JsonConvert.SerializeObject(value, _jsonSettings);
    }
}