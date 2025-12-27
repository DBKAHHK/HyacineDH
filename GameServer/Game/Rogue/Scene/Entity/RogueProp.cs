using EggLink.DanhengServer.Data.Config.Scene;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.RogueMagic;
using EggLink.DanhengServer.Enums.TournRogue;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.Rogue.Scene.Entity;

public class RogueProp(SceneInstance scene, MazePropExcel excel, GroupInfo group, PropInfo prop)
    : EntityProp(scene, excel, group, prop)
{
    public RogueProp(EntityProp prop) : this(prop.Scene, prop.Excel, prop.Group, prop.PropInfo)
    {
    }

    public int NextRoomId { get; set; }
    public int NextSiteId { get; set; }
    public int ChestCanUseTimes { get; set; }
    public int CustomPropId { get; set; }

    public bool IsChessRogue { get; set; } = false;
    public bool IsLastRoom { get; set; } = false;

    public bool IsTournRogue { get; set; } = false;
    public bool EnterNextLayer { get; set; } = false;
    public RogueTournRoomTypeEnum RoomType { get; set; } = RogueTournRoomTypeEnum.Unknown;

    public bool IsMagicRogue { get; set; } = false;
    public RogueMagicRoomTypeEnum MagicRoomType { get; set; } = RogueMagicRoomTypeEnum.Unknown;

    public override SceneEntityInfo ToProto()
    {
        var proto = base.ToProto();

        if (NextRoomId != 0 || NextSiteId != 0 || ChestCanUseTimes != 0) // do not set if all are 0
            proto.Prop.ExtraInfo = new PropExtraInfo
            {
                // RogueGameInfo 原名为 RogueInfo
                //RogueGameInfo = new PropRogueInfo
                //{
                //    RoomId = (uint)NextRoomId,
                //    SiteId = (uint)NextSiteId
                //} 
            };

        if (IsChessRogue)
            proto.Prop.ExtraInfo = new PropExtraInfo
            {
                // PropChessRogueInfo 原名为 ChessRogueInfo
                //PropChessRogueInfo = new PropChessRogueInfo
                //{
                //    EnterNextCell = !IsLastRoom
                //}
            };

        if (IsTournRogue)
            proto.Prop.ExtraInfo = new PropExtraInfo
            {
                // PropRogueTournDoorInfo 原名为 RogueTournDoorInfo
                //PropRogueTournDoorInfo = new RogueTournDoorInfo
                //{
                //    EnterNextLayer = EnterNextLayer,
                //    RogueDoorNextRoomType = (uint)RoomType
                //}
            };

        if (IsMagicRogue)
            proto.Prop.ExtraInfo = new PropExtraInfo
            {
                // PropRogueMagicDoorInfo 原名为 RogueMagicDoorInfo
                //PropRogueMagicDoorInfo = new RogueMagicDoorInfo
                //{
                //    EnterNextLayer = EnterNextLayer,
                //    RogueDoorNextRoomType = (uint)MagicRoomType
                //}
            };

        if (CustomPropId != 0) proto.Prop.PropId = (uint)CustomPropId;

        return proto;
    }
}