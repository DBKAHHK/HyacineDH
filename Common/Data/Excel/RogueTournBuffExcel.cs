using EggLink.DanhengServer.Data.Custom;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RogueTournBuff.json")]
public class RogueTournBuffExcel : BaseRogueBuffExcel
{
    public bool IsInHandbook { get; set; }

    public override void Loaded()
    {
        GameData.RogueBuffData.TryAdd(GetId(), this);
    }
}