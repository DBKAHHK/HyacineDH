using EggLink.DanhengServer.Enums.Rogue;
using EggLink.DanhengServer.Enums.TournRogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RogueTournRoom.json")]
public class RogueTournRoomExcel : ExcelResource
{
    public uint RogueRoomID { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTournRoomTypeEnum RogueRoomType { get; set; }


    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTournModeEnum RogueTournMode { get; set; }


    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTournVariantTypeEnum VariantType { get; set; }


    public override int GetId()
    {
        return (int)RogueRoomID;
    }

    public override void Loaded()
    {
        GameData.RogueTournRoomData.Add(RogueRoomID, this);
    }
}