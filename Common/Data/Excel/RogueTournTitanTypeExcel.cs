using EggLink.DanhengServer.Enums.TournRogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RogueTournTitanType.json")]
public class RogueTournTitanTypeExcel : ExcelResource
{
    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTitanTypeEnum RogueTitanType { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTitanCategoryEnum RogueTitanCategory { get; set; }

    public override int GetId()
    {
        return (int)RogueTitanType;
    }

    public override void Loaded()
    {
        GameData.RogueTournTitanTypeData.Add(RogueTitanType, this);
    }
}