namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("MonsterConfig.json")]
public class MonsterConfigExcel : ExcelResource
{
    public int MonsterID { get; set; }
    public int MonsterTemplateID { get; set; }
    public int EliteGroup { get; set; }
    public int HardLevelGroup { get; set; }

    public override int GetId()
    {
        return MonsterID;
    }

    public override void Loaded()
    {
        if (!GameData.MonsterConfigData.TryAdd(MonsterID, this))
            GameData.MonsterConfigData[MonsterID] = this;
    }
}
