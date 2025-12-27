using EggLink.DanhengServer.Data.Config.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Entity;

public class EntityNpc(SceneInstance scene, GroupInfo group, NpcInfo npcInfo) : BaseGameEntity
{
    public SceneInstance Scene { get; set; } = scene;
    public Position Position { get; set; } = npcInfo.ToPositionProto();
    public Position Rotation { get; set; } = npcInfo.ToRotationProto();
    public int NpcId { get; set; } = npcInfo.NPCID;
    public int InstId { get; set; } = npcInfo.ID;
    public override int EntityId { get; set; }
    public override int GroupId { get; set; } = group.Id;

    public override SceneEntityInfo ToProto()
    {
        SceneNpcInfo npc = new()
        {
            NpcId = (uint)NpcId
        };

        return new SceneEntityInfo
        {
            EntityId = (uint)EntityId,
            GroupId = (uint)GroupId,
            Motion = new MotionInfo
            {
                Pos = Position.ToProto(),
                Rot = Rotation.ToProto()
            },
            InstId = (uint)InstId,
            Npc = npc
        };
    }
}