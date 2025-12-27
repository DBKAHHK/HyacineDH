using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using Google.Protobuf;
using SqlSugar;

namespace EggLink.DanhengServer.Database.Scene;

[SugarTable("Scene")]
public class SceneData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, Dictionary<int, List<ScenePropData>>> ScenePropData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<GroupId, ScenePropData>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, List<int>> UnlockSectionIdList { get; set; } = []; // Dictionary<FloorId, List<SectionId>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, Dictionary<int, string>> CustomSaveData { get; set; } =
        []; // Dictionary<EntryId, Dictionary<GroupId, SaveData>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, Dictionary<string, int>> FloorSavedData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<SaveDataKey, SaveDataValue>>

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, Dictionary<int, Dictionary<int, ScenePropTimelineData>>> PropTimelineData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<GroupId, Dictionary<PropId, ScenePropTimelineData>>>

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, List<SceneMarkedChestData>> MarkedChestData { get; set; } =
        []; // Dictionary<FuncId, List<ScenePropTimelineData>>

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, Dictionary<int, Dictionary<string, int>>> GroupPropertyData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<GroupId, Dictionary<Key, Value>>>

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public SceneEraFlipperData EraFlipperData { get; set; } = new();

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public SceneRotatableRegionData RotatableRegionData { get; set; } = new();

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, int> FloorTargetPuzzleGroupData { get; set; } = new();

    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, SwitchHandInfo> SwitchHandData { get; set; } = new();

    public int GetFloorSavedValue(int floorId, string key)
    {
        if (FloorSavedData.TryGetValue(floorId, out var data) && data.TryGetValue(key, out var value))
        {
            return value;
        }

        // get default value if not found
        var floor = GameData.GetFloorInfo(floorId);
        if (floor == null) return 0;

        var savedValue = floor.FloorSavedValue.FirstOrDefault(x => x.Name == key);
        return savedValue?.DefaultValue ?? 0;
    }

    public Dictionary<string, int> GetFloorSavedValues(int floorId)
    {
        var floor = GameData.GetFloorInfo(floorId);
        if (floor == null) return [];

        var savedValues = new Dictionary<string, int>();
        foreach (var value in floor.FloorSavedValue)
        {
            savedValues[value.Name] = GetFloorSavedValue(floorId, value.Name);
        }

        return savedValues;
    }
}

public class SwitchHandInfo
{
    public int ConfigId { get; set; }
    public int CoinNum { get; set; }
    public Position Pos { get; set; } = new();
    public Position Rot { get; set; } = new();
    public uint State { get; set; } = 101;
    public byte[] ByteValue { get; set; } = [];

    public HandInfo ToProto()
    {
        return new HandInfo
        {
            ConfigId = (uint)ConfigId,
            HandByteValue = ByteString.CopyFrom(ByteValue),
            HandCoinNum = (uint)CoinNum,
            HandMotion = new MotionInfo
            {
                Pos = Pos.ToProto(),
                Rot = Rot.ToProto()
            },
            HandState = State
        };
    }
}

public class ScenePropData
{
    public int PropId { get; set; }
    public PropStateEnum State { get; set; }
}

public class SceneEraFlipperData
{
    public int CurRegionId { get; set; }
    public Dictionary<int, int> RegionState { get; set; } = []; // Dictionary<RegionId, State>
}

public class SceneRotatableRegionData
{
    public int CurRegionId { get; set; }
    public int Energy { get; set; }
    public int MaxEnergy { get; set; }
    public int RotateValue { get; set; }
}

public class ScenePropTimelineData
{
    public bool BoolValue { get; set; }
    public string ByteValue { get; set; } = ""; // Base64

    public PropTimelineInfo ToProto()
    {
        return new PropTimelineInfo
        {
            TimelineBoolValue = BoolValue,
            TimelineByteValue = ByteString.FromBase64(ByteValue)
        };
    }
}

public class SceneMarkedChestData
{
    public int ConfigId { get; set; }
    public int GroupId { get; set; }
    public int FloorId { get; set; }
    public int PlaneId { get; set; }

    public MarkChestInfo ToProto()
    {
        return new MarkChestInfo
        {
            ConfigId = (uint)ConfigId,
            FloorId = (uint)FloorId,
            GroupId = (uint)GroupId,
            PlaneId = (uint)PlaneId
        };
    }
}