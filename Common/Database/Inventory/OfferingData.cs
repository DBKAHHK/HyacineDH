using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Proto;
using SqlSugar;

namespace EggLink.DanhengServer.Database.Inventory;

[SugarTable("offering_data")]
public class OfferingData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, OfferingTypeData> Offerings { get; set; } = [];
}

public class OfferingTypeData
{
    public OfferingState State { get; set; } = OfferingState.Open;
    public int CurExp { get; set; }
    public int OfferingId { get; set; }
    public int Level { get; set; }
    public List<int> TakenReward { get; set; } = [];

    public OfferingInfo ToProto()
    {
        var totalExp = CurExp + Enumerable.Range(1, Level)
            .Select(level => GameData.OfferingLevelConfigData.GetValueOrDefault(OfferingId)?.GetValueOrDefault(level))
            .OfType<OfferingLevelConfigExcel>().Sum(config => config.ItemCost);

        return new OfferingInfo
        {
            OfferingState = State,
            HasTakenRewardIdList = { TakenReward.Select(x => (uint)x) },
            LevelExp = (uint)CurExp,
            OfferingId = (uint)OfferingId,
            OfferingLevel = (uint)Level,
            TotalExp = (uint)totalExp
        };
    }
}