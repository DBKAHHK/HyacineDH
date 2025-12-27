using EggLink.DanhengServer.Enums.Avatar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Config.Task;

public class ByCharacterDamageType : PredicateConfigInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public DamageTypeEnum DamageType { get; set; } = DamageTypeEnum.Fire;
}