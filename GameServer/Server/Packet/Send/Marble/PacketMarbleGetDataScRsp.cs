using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Marble;

public class PacketMarbleGetDataScRsp : BasePacket
{
    public PacketMarbleGetDataScRsp() : base(CmdIds.MarbleGetDataScRsp)
    {
        var proto = new MarbleGetDataScRsp
        {
            OwnedSealList = { GameData.MarbleSealData.Keys.Select(x => (uint)x) },
            MarbleFinishLevelIdList = { GameData.MarbleMatchInfoData.Keys.Select(x => (uint)x) }
        };

        SetData(proto);
    }
}