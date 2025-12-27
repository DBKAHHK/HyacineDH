using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;

public class PacketFightGeneralScNotify : BasePacket
{
public PacketFightGeneralScNotify(MarbleNetWorkMsgEnum msgType, MarbleNetWorkMsgEnum syncType,
    MarbleGameRoomInstance game) : base(CmdIds.FightGeneralScNotify)
{
    var proto = new FightGeneralScNotify
    {
        // AHLHKGLPHFM 原名为 NetworkMsgType，需要转换为 ByteString
        AHLHKGLPHFM = Google.Protobuf.ByteString.CopyFrom(BitConverter.GetBytes((uint)msgType)),
        // NAFFAFEHPFK 原名为 FightGeneralInfo，需要转换为 uint
        NAFFAFEHPFK = (uint)new FightGeneralServerInfo
        {
            FightGameInfo =
            {
                new FightGameInfo
                {
                    MarbleGameInfo = game.ToProto(),
                    GameMessageType = (uint)syncType
                }
            }
        }.GetHashCode()
    };

    SetData(proto);
}





public PacketFightGeneralScNotify(MarbleNetWorkMsgEnum msgType, MarbleNetWorkMsgEnum syncType,
    FightMarbleSealInfo sealInfo) : base(CmdIds.FightGeneralScNotify)
{
    var proto = new FightGeneralScNotify
    {
        // AHLHKGLPHFM 原名为 NetworkMsgType，需要转换为 ByteString
        AHLHKGLPHFM = Google.Protobuf.ByteString.CopyFrom(BitConverter.GetBytes((uint)msgType)),
        // NAFFAFEHPFK 原名为 FightGeneralInfo，需要转换为 uint
        NAFFAFEHPFK = (uint)new FightGeneralServerInfo
        {
            FightGameInfo =
            {
                new FightGameInfo
                {
                    FightMarbleSealInfo = sealInfo,
                    GameMessageType = (uint)syncType
                }
            }
        }.GetHashCode()
    };

    SetData(proto);
}


public PacketFightGeneralScNotify(MarbleNetWorkMsgEnum msgType, List<MarbleGameBaseSyncData> sync) : base(
    CmdIds.FightGeneralScNotify)
{
    var proto = new FightGeneralScNotify
    {
        // AHLHKGLPHFM 原名为 NetworkMsgType，需要转换为 ByteString
        AHLHKGLPHFM = Google.Protobuf.ByteString.CopyFrom(BitConverter.GetBytes((uint)msgType)),
        // NAFFAFEHPFK 原名为 FightGeneralInfo，需要转换为 uint
        NAFFAFEHPFK = (uint)new FightGeneralServerInfo
        {
            FightGameInfo = { sync.Select(x => x.ToProto()) }
        }.GetHashCode()
    };

    SetData(proto);
}
}