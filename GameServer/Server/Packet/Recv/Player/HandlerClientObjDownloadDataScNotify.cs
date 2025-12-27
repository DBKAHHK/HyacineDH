using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.ClientObjDownloadDataScNotify)]
public class HandlerClientObjDownloadDataScNotify : Handler
{
    public override Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        _ = ClientObjDownloadDataScNotify.Parser.ParseFrom(data);
        return Task.CompletedTask;
    }
}

