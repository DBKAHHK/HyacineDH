using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Enums.Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("AvatarConfig.json,AvatarConfigTrial.json,AvatarConfigLD.json", true)]
public class AvatarConfigExcel : ExcelResource
{
    [JsonIgnore] public Dictionary<int, List<AvatarSkillTreeConfigExcel>> DefaultSkillTree { get; set; } = [];
    [JsonIgnore] public string? Name { get; set; }
    [JsonIgnore] public Dictionary<int, List<AvatarSkillTreeConfigExcel>> SkillTree { get; set; } = [];

    public int AvatarID { get; set; } = 0;
    public int AdventurePlayerID { get; set; }
    public HashName AvatarName { get; set; } = new();
    public int ExpGroup { get; set; } = 0;
    public int MaxPromotion { get; set; } = 0;
    public int MaxRank { get; set; } = 0;
    public List<int> RankIDList { get; set; } = [];
    public string? JsonPath { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    public RarityEnum Rarity { get; set; } = 0;

    [JsonConverter(typeof(StringEnumConverter))]
    public DamageTypeEnum DamageType { get; set; } = 0;

    public override int GetId()
    {
        return AvatarID;
    }

    public override void Loaded()
    {
        if (!GameData.AvatarConfigData.ContainsKey(AvatarID)) GameData.AvatarConfigData.Add(AvatarID, this);
        JsonPath = null;
    }
}