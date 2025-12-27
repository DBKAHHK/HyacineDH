using EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Fight;

[Opcode(CmdIds.FightGeneralCsReq)]
public class HandlerFightGeneralCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FightGeneralCsReq.Parser.ParseFrom(data);

        if (connection.MarbleRoom == null || connection.MarblePlayer == null)
        {
            await connection.SendPacket(new PacketFightGeneralScRsp(Retcode.RetFightRoomNotExist));
            return;
        }

        // AHLHKGLPHFM 原名为 NetworkMsgType，需要从 ByteString 转换为 uint
        uint msgType = BitConverter.ToUInt32(req.AHLHKGLPHFM.ToByteArray());
        
        // NAFFAFEHPFK 原名为 FightGeneralInfo，需要将 uint 转换为 byte[]
        await connection.MarbleRoom.HandleGeneralRequest(connection.MarblePlayer, msgType, 
            BitConverter.GetBytes(req.NAFFAFEHPFK));

        await connection.SendPacket(new PacketFightGeneralScRsp(msgType));


    }
}