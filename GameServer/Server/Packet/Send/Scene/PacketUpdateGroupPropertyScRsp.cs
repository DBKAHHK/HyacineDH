using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketUpdateGroupPropertyScRsp : BasePacket
{
    public PacketUpdateGroupPropertyScRsp(Retcode code) : base(CmdIds.UpdateGroupPropertyScRsp)
    {
        var proto = new UpdateGroupPropertyScRsp
        {
            Retcode = (uint)code
        };

        SetData(proto);
    }

    public PacketUpdateGroupPropertyScRsp(GroupPropertyRefreshData data, UpdateGroupPropertyCsReq req) : base(CmdIds.UpdateGroupPropertyScRsp)
    {
        var proto = new UpdateGroupPropertyScRsp
        {
            DimensionId = req.DimensionId,
            FloorId = req.FloorId,
            GroupId = (uint)data.GroupId,
            JOFPDCALAGA = data.NewValue,
            DKDBCCACMCG = data.OldValue,
            DELEKFMGGCM = data.PropertyName
        };

        SetData(proto);
    }
}