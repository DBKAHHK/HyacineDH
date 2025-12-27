namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("OfferingLevelConfig.json")]
public class OfferingLevelConfigExcel : ExcelResource
{
    public int ItemCost { get; set; }
    public int Level { get; set; }
    public int RewardID { get; set; }
    public int TypeID { get; set; }
    public int UnlockID { get; set; }

    public override int GetId()
    {
        return TypeID * 1000 + Level;
    }

    public override void Loaded()
    {
        GameData.OfferingLevelConfigData.TryAdd(TypeID, []);
        GameData.OfferingLevelConfigData[TypeID].Add(Level, this);
    }
}