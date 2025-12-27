using EggLink.DanhengServer.Enums.Avatar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("SpecialAvatar.json")]
public class SpecialAvatarExcel : ExcelResource
{
    public int SpecialAvatarID { get; set; }
    public int WorldLevel { get; set; }
    public int AvatarID { get; set; }
    public int PlayerID { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public SpecialAvatarTypeEnum Type { get; set; }

    public int Level { get; set; }
    public int Promotion { get; set; }
    public int Rank { get; set; }
    public int EquipmentID { get; set; }
    public int EquipmentLevel { get; set; }
    public int EquipmentPromotion { get; set; }
    public int EquipmentRank { get; set; }
    public int RelicPropertyType { get; set; }
    public int RelicPropertyTypeExtra { get; set; }

    public override int GetId()
    {
        return SpecialAvatarID * 10 + WorldLevel;
    }

    public override void Loaded()
    {
        GameData.SpecialAvatarData[GetId()] = this;
    }

    public override void AfterAllDone()
    {
    }
}