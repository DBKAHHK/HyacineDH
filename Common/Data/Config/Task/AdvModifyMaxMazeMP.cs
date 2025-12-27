using EggLink.DanhengServer.Enums.Avatar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Config.Task;

public class AdvModifyMaxMazeMP : TaskConfigInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public PropertyModifyFunctionEnum ModifyFunction { get; set; }

    public DynamicFloat ModifyValue { get; set; } = new();
}