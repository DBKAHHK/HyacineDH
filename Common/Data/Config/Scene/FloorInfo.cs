using System.Collections.Concurrent;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Config.Scene;

public class FloorInfo
{
    [JsonConverter(typeof(ConcurrentDictionaryConverter<int, PropInfo>))]
    public ConcurrentDictionary<int, PropInfo> CachedTeleports = [];

    [JsonConverter(typeof(ConcurrentDictionaryConverter<int, GroupInfo>))]
    public ConcurrentDictionary<int, GroupInfo> Groups = [];

    [JsonIgnore] public bool Loaded;

    [JsonConverter(typeof(ConcurrentBagConverter<int>))]
    public ConcurrentBag<int> MapSections = [];

    [JsonConverter(typeof(ConcurrentBagConverter<PropInfo>))]
    public ConcurrentBag<PropInfo> UnlockedCheckpoints = [];

    public int FloorID { get; set; }
    public int StartGroupIndex { get; set; }
    public int StartAnchorID { get; set; }
    public string NavmapConfigPath { get; set; } = "";
    public List<FloorGroupInfo> GroupInstanceList { get; set; } = [];
    public List<FloorDimensionInfo> DimensionList { get; set; } = [];

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<LevelFeatureTypeEnum> LevelFeatureModules { get; set; } = [];

    [JsonIgnore] public int StartGroupID { get; set; }
    [JsonIgnore] public List<FloorSavedValueInfo> FloorSavedValue { get; set; } = [];

    public AnchorInfo? GetAnchorInfo(int groupId, int anchorId)
    {
        Groups.TryGetValue(groupId, out var group);
        if (group == null) return null;

        return group.AnchorList.Find(info => info.ID == anchorId);
    }


    public void OnLoad()
    {
        if (Loaded) return;

        StartGroupID = GroupInstanceList[StartGroupIndex].ID;

        foreach (var dimension in DimensionList) dimension.OnLoad(this);

        // Cache anchors
        foreach (var group in Groups.Values)
        foreach (var prop in group.PropList)
            // Check if prop can be teleported to
            if (prop.AnchorID > 0)
            {
                // Put inside cached teleport list to send to client when they request map info
                CachedTeleports.TryAdd(prop.MappingInfoID, prop);
                UnlockedCheckpoints.Add(prop);

                // Force prop to be in the unlocked state
                prop.State = PropStateEnum.CheckPointEnable;
            }
            else if (!string.IsNullOrEmpty(prop.InitLevelGraph))
            {
                var json = prop.InitLevelGraph;

                // Hacky way to setup prop triggers
                if (json.Contains("Maze_GroupProp_OpenTreasure_WhenMonsterDie"))
                {
                    //prop.Trigger = new TriggerOpenTreasureWhenMonsterDie(group.Id);
                }
                else if (json.Contains("Common_Console"))
                {
                    prop.CommonConsole = true;
                }

                // Clear for garbage collection
                prop.ValueSource = null;
                prop.InitLevelGraph = null;
            }

        Loaded = true;
    }
}

public class FloorGroupInfo
{
    public string GroupPath { get; set; } = "";
    public bool IsDelete { get; set; }
    public int ID { get; set; }
    public string Name { get; set; } = "";
}

public class FloorSavedValueInfo
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DefaultValue { get; set; }
}

public class FloorDimensionInfo
{
    public int ID { get; set; }
    public List<DimensionSavedValues> SavedValues { get; set; } = [];
    public List<int> GroupIndexList { get; set; } = [];

    [JsonIgnore] public List<int> GroupIDList { get; set; } = [];

    public void OnLoad(FloorInfo floor)
    {
        foreach (var data in SavedValues)
        {
            if (floor.FloorSavedValue.Any(x => x.Name == data.Name)) continue;

            floor.FloorSavedValue.Add(new FloorSavedValueInfo
            {
                ID = data.ID,
                Name = data.Name,
                DefaultValue = data.MaxValue
            });
        }

        foreach (var index in GroupIndexList) GroupIDList.Add(floor.GroupInstanceList[index].ID);
    }
}

public class DimensionSavedValues
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxValue { get; set; }
}