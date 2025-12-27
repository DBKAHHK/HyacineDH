using EggLink.DanhengServer.Enums.TournRogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RogueTournTitanTalent.json")]
public class RogueTournTitanTalentExcel : ExcelResource
{
    public int ID { get; set; }
    public int PreID { get; set; }
    public int Level { get; set; }
    public List<MappingInfoItem> Cost { get; set; } = [];

    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTitanTypeEnum RogueTitanType { get; set; }

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.RogueTournTitanTalentData.Add(ID, this);
    }
}