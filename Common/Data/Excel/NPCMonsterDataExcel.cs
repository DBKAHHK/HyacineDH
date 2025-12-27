using EggLink.DanhengServer.Enums.Scene;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("NPCMonsterData.json")]
public class NPCMonsterDataExcel : ExcelResource
{
    public int ID { get; set; }
    public HashName NPCName { get; set; } = new();
    public string JsonPath { get; set; } = "";
    public string ConfigEntityPath { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    public MonsterRankEnum Rank { get; set; }

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.NpcMonsterDataData.Add(ID, this);
    }
}