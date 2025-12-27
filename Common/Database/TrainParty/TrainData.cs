using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;
using SqlSugar;

namespace EggLink.DanhengServer.Database.TrainParty;

[SugarTable("TrainParty")]
public class TrainData : BaseDatabaseDataHelper
{
    public int Fund { get; set; }
    [SugarColumn(IsJson = true)] public Dictionary<int, TrainAreaInfo> Areas { get; set; } = [];
}

public class TrainAreaInfo
{
    public int AreaId { get; set; }
    public List<int> StepList { get; set; } = [];
    public Dictionary<int, int> DynamicInfo { get; set; } = [];

    public TrainPartyArea ToProto()
    {
        var info = new TrainPartyArea
        {
            AreaId = (uint)AreaId,
            AreaStepInfo = new AreaStepInfo(),
            StepIdList = { StepList.Select(x => (uint)x) },
            VerifyStepIdList = { StepList.Select(x => (uint)x) },
            Progress = 100,
            DynamicInfo =
            {
                DynamicInfo.Select(x => new AreaDynamicInfo
                {
                    DiceSlotId = (uint)x.Key,
                    DiyDynamicId = (uint)x.Value
                })
            }
        };

        foreach (var step in StepList)
        {
            GameData.TrainPartyStepConfigData.TryGetValue(step, out var stepExcel);
            if (stepExcel == null) continue;

            info.StaticPropIdList.AddRange(stepExcel.StaticPropIDList.Select(x => (uint)x));
        }

        return info;
    }
}