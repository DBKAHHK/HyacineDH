using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Music;

public class PacketMusicRhythmUnlockTrackScNotify : BasePacket
{
    public PacketMusicRhythmUnlockTrackScNotify() : base(CmdIds.MusicRhythmUnlockTrackScNotify)
    {
        var proto = new MusicRhythmUnlockTrackScNotify();

        // LPOIGAOLEJE 原名为 TrackUnlockList
        foreach (var sfx in GameData.MusicRhythmTrackData.Values) proto.LPOIGAOLEJE.Add((uint)sfx.GetId());

        SetData(proto);
    }
}