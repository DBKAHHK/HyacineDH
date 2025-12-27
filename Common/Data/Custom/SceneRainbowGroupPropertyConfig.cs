using EggLink.DanhengServer.Enums.Scene;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Custom;

public class SceneRainbowGroupPropertyConfig
{
    public Dictionary<int, Dictionary<int, Dictionary<string, Dictionary<int, RainbowGroupPropertyInfo>>>> FloorProperty
    {
        get;
        set;
    } = [];
}

public class RainbowGroupPropertyInfo
{
    public List<RainbowActionInfo> PrivateActions { get; set; } = [];
    public List<RainbowActionInfo> Actions { get; set; } = [];
}

public class RainbowActionInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public SceneActionTypeEnum ActionType { get; set; } = SceneActionTypeEnum.Unknown;

    public Dictionary<string, object> Params { get; set; } = [];
}