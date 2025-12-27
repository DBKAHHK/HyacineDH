using EggLink.DanhengServer.Data.Config;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("SubMission.json")]
public class SubMissionExcel : ExcelResource
{
    public int SubMissionID { get; set; }

    public HashName TargetText { get; set; } = new();

    [JsonIgnore] public int MainMissionID { get; set; }

    [JsonIgnore] public MissionInfo? MainMissionInfo { get; set; }

    [JsonIgnore] public SubMissionInfo? SubMissionInfo { get; set; }

    [JsonIgnore] public LevelGraphConfigInfo? SubMissionTaskInfo { get; set; }

    public override int GetId()
    {
        return SubMissionID;
    }

    public override void Loaded()
    {
        GameData.SubMissionData[GetId()] = this;
    }
}

public class SubMissionData(int missionId)
{
    public int MissionId { get; set; } = missionId;
    public int MainMissionId { get; set; }

    public MissionInfo? MainMissionInfo { get; set; }

    public SubMissionInfo? SubMissionInfo { get; set; }

    public LevelGraphConfigInfo? SubMissionTaskInfo { get; set; }
}