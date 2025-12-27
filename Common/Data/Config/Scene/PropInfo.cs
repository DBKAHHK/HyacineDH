using EggLink.DanhengServer.Enums.Scene;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.Data.Config.Scene;

public class PropInfo : PositionInfo
{
    [JsonIgnore] public bool CommonConsole = false;
    public int MappingInfoID { get; set; }
    public int AnchorGroupID { get; set; }
    public int AnchorID { get; set; }
    public int PropID { get; set; }
    public int EventID { get; set; }
    public int CocoonID { get; set; }
    public int ChestID { get; set; }
    public int FarmElementID { get; set; }
    public bool IsClientOnly { get; set; }
    public bool LoadOnInitial { get; set; }

    public ValueSourceInfo? ValueSource { get; set; }
    public string? InitLevelGraph { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public PropStateEnum State { get; set; } = PropStateEnum.Closed;

    [JsonIgnore] public Dictionary<int, List<int>> UnlockDoorID { get; set; } = [];

    [JsonIgnore] public Dictionary<int, List<int>> UnlockControllerID { get; set; } = [];

    [JsonIgnore] public bool IsLevelBtn { get; set; }

    public void Load(GroupInfo info)
    {
        if (ValueSource == null) return;

        if (Name.StartsWith("Button_") &&
            ValueSource.Values.Find(x => x["Key"]?.ToString() == "AnchorName") != null)
            IsLevelBtn = true;

        foreach (var v in ValueSource.Values)
        {
            var key = v["Key"];
            var value = v["Value"];
            if (value == null || key == null) continue;

            if (key.ToString() == "ListenTriggerCustomString")
            {
                if (!info.PropTriggerCustomString.TryGetValue(value.ToString(), out var list))
                {
                    list = [];
                    info.PropTriggerCustomString.Add(value.ToString(), list);
                }

                list.Add(ID);
            }
            else if (key.ToString().Contains("Door") ||
                     key.ToString().Contains("Bridge") ||
                     key.ToString().Contains("UnlockTarget") ||
                     key.ToString().Contains("Rootcontamination") ||
                     key.ToString().Contains("Portal"))
            {
                var parts = value.ToString().Split(',');
                if (parts.Length >= 2 &&
                    int.TryParse(parts[0], out var keyId) &&
                    int.TryParse(parts[1], out var valueId))
                {
                    if (!UnlockDoorID.ContainsKey(keyId))
                        UnlockDoorID.Add(keyId, []);
                    UnlockDoorID[keyId].Add(valueId);
                }
            }
            else if (key.ToString().Contains("Controller"))
            {
                var parts = value.ToString().Split(',');
                if (parts.Length >= 2 &&
                    int.TryParse(parts[0], out var keyId) &&
                    int.TryParse(parts[1], out var valueId))
                {
                    if (!UnlockControllerID.ContainsKey(keyId))
                        UnlockControllerID.Add(keyId, []);
                    UnlockControllerID[keyId].Add(valueId);
                }
            }
        }
    }
}

public class ValueSourceInfo
{
    public List<JObject> Values { get; set; } = [];
}