using EggLink.DanhengServer.Proto;
using Google.Protobuf;

namespace EggLink.DanhengServer.Kcp;

public class HandshakePacket : BasePacket
{
    public HandshakePacket(byte[] data) : base(CmdIds.ClientDownloadDataScNotify)
    {
        var downloadData = new ClientDownloadData
        {
            Data = ByteString.CopyFrom(data),
            Version = 81,
            Time = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds
        };
        var notify = new ClientDownloadDataScNotify
        {
            DownloadData = downloadData
        };

        SetData(notify);
    }

    public HandshakePacket(string base64) : base(CmdIds.ClientDownloadDataScNotify)
    {
        SetData(Convert.FromBase64String(base64));
    }
}