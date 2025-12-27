using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using Google.Protobuf;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketClientObjUploadScRsp : BasePacket
{
    public PacketClientObjUploadScRsp(EJLBONKELAO contentType, uint retcode = 0) : base(CmdIds.ClientObjUploadScRsp)
    {
        var proto = new ClientObjUploadScRsp
        {
            Retcode = retcode,
            Data = new ClientObjDownloadData
            {
                ClientObjDownloadData_ = new ClientDownloadData
                {
                    Version = 1,
                    Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Data = ByteString.Empty
                }
            }
        };

        proto.Data.LIMLHJPIEOC = ByteString.Empty;
        proto.Data.HGJOOEPCBKM.Add(proto.Data.ClientObjDownloadData_);

        SetData(proto);
    }
}

