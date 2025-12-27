using Newtonsoft.Json;

namespace EggLink.DanhengServer.Data.Custom;

public class BaseRogueBuffGroupExcel : ExcelResource
{
    public int GroupId { get; set; }
    [JsonIgnore] public List<BaseRogueBuffExcel> BuffList { get; set; } = [];
    [JsonIgnore] public bool IsLoaded { get; set; }

    public override int GetId()
    {
        return GroupId;
    }
}