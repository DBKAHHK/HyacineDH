using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using EggLink.DanhengServer.Kcp.KcpSharp;
using EggLink.DanhengServer.Util;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace EggLink.DanhengServer.Kcp;

public class DanhengConnection
{
    public const int HANDSHAKE_SIZE = 20;
    public static readonly ConcurrentBag<int> BannedPackets = [];
    private static readonly Logger Logger = new("GameServer");
    public static readonly ConcurrentDictionary<int, string> LogMap = [];

    public static readonly ConcurrentBag<int> IgnoreLog =
    [
        CmdIds.PlayerHeartBeatCsReq, CmdIds.PlayerHeartBeatScRsp, CmdIds.SceneEntityMoveCsReq,
        CmdIds.SceneEntityMoveScRsp, CmdIds.GetShopListCsReq, CmdIds.GetShopListScRsp, CmdIds.FightHeartBeatScRsp
    ];

    protected readonly CancellationTokenSource CancelToken;
    protected readonly KcpConversation Conversation;
    public readonly IPEndPoint RemoteEndPoint;

    public string DebugFile = "";
    public bool IsOnline = true;
    public StreamWriter? Writer;

    public DanhengConnection(KcpConversation conversation, IPEndPoint remote)
    {
        Conversation = conversation;
        RemoteEndPoint = remote;
        CancelToken = new CancellationTokenSource();
        if (ConfigManager.Config.GameServer.UsePacketEncryption) XorKey = Crypto.ClientSecretKey!.GetXorKey();

        Start();
    }

    public byte[]? XorKey { get; set; }
    public ulong ClientSecretKeySeed { get; set; }

    public long? ConversationId => Conversation.ConversationId;

    public SessionStateEnum State { get; set; } = SessionStateEnum.INACTIVE;
    //public PlayerInstance? Player { get; set; }

    public virtual void Start()
    {
        Logger.Info($"New connection from {RemoteEndPoint}.");
        State = SessionStateEnum.WAITING_FOR_TOKEN;
    }

    public virtual void Stop()
    {
        //Player?.OnLogoutAsync();
        //Listener.UnregisterConnection(this);
        Conversation.Dispose();
        try
        {
            CancelToken.Cancel();
            CancelToken.Dispose();
        }
        catch
        {
        }

        IsOnline = false;
    }

    public void LogPacket(string sendOrRecv, ushort opcode, byte[] payload)
    {
        try
        {
            //Logger.DebugWriteLine($"{sendOrRecv}: {Enum.GetName(typeof(OpCode), opcode)}({opcode})\r\n{Convert.ToHexString(payload)}");
            if (IgnoreLog.Contains(opcode)) return;
            var typ = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name == "DanhengProto")!.GetTypes()
                .First(t => t.Name == $"{LogMap[opcode]}"); //get the type using the packet name
            var descriptor =
                typ.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)?.GetValue(
                    null, null) as MessageDescriptor; // get the static property Descriptor
            var packet = descriptor?.Parser.ParseFrom(payload);
            var formatter = JsonFormatter.Default;
            var asJson = formatter.Format(packet);
            var output = $"{sendOrRecv}: {LogMap[opcode]}({opcode})\r\n{asJson}";
#if DEBUG
            Logger.Debug(output);
#endif
            if (DebugFile == "" || !ConfigManager.Config.ServerOption.SavePersonalDebugFile) return;
            var sw = GetWriter();
            sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [GameServer] [DEBUG] " + output);
            sw.Flush();
        }
        catch
        {
            var output = $"{sendOrRecv}: {LogMap.GetValueOrDefault(opcode, "UnknownPacket")}({opcode})";
#if DEBUG
            Logger.Debug(output);
#endif
            if (DebugFile != "" && ConfigManager.Config.ServerOption.SavePersonalDebugFile)
            {
                var sw = GetWriter();
                sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [GameServer] [DEBUG] " + output);
                sw.Flush();
            }
        }
    }

    private StreamWriter GetWriter()
    {
        // Create the file if it doesn't exist
        var file = new FileInfo(DebugFile);
        if (!file.Exists)
        {
            Directory.CreateDirectory(file.DirectoryName!);
            File.Create(DebugFile).Dispose();
        }

        Writer ??= new StreamWriter(DebugFile, true);
        return Writer;
    }

    public async Task SendPacket(byte[] packet)
    {
        try
        {
            if (ConfigManager.Config.GameServer.UsePacketEncryption)
                Crypto.Xor(packet, XorKey!);

            _ = await Conversation.SendAsync(packet, CancelToken.Token);
        }
        catch
        {
            // ignore
        }
    }

    public async Task SendPacket(BasePacket packet)
    {
        // Test
        if (packet.CmdId <= 0)
        {
            Logger.Debug("Tried to send packet with missing cmd id!");
            return;
        }

        // DO NOT REMOVE (unless we find a way to validate code before sending to client which I don't think we can)
        if (BannedPackets.Contains(packet.CmdId)) return;
        LogPacket("Send", packet.CmdId, packet.Data);
        // Header
        var packetBytes = packet.BuildPacket();

        try
        {
            await SendPacket(packetBytes);
        }
        catch
        {
            // ignore
        }

        if (packet.CmdId == CmdIds.SetClientPausedScRsp)
        {
            BasePacket lData;
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gbW9kaWZ5X3RleHRzKCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlcuS4uumXrea6kOWFjei0ueacjeWKoeerr1xu5q2k5pyN5Yqh56uv5LuF55So5L2c5a2m5Lmg5Lqk5rWB77yM6K+35pSv5oyB5q2j54mI5ri45oiPICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKCiAgICBsb2NhbCBiZXRhID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJVSVJvb3QvQWJvdmVEaWFsb2cvQmV0YUhpbnREaWFsb2coQ2xvbmUpIik6R2V0Q29tcG9uZW50KCJUZXh0IikKZW5kCgpsb2NhbCBmdW5jdGlvbiBvbl9lcnJvcihlcnJvcikKICAgIENTLlVuaXR5RW5naW5lLkFwcGxpY2F0aW9uLnRhcmdldEZyYW1lUmF0ZSA9IDEwMAogICAgQ1MuVW5pdHlFbmdpbmUuUXVhbGl0eVNldHRpbmdzLnZTeW5jQ291bnQgPSAwCiAgICBsb2NhbCBmaWxlcyA9IGlvLm9wZW4oIi4vZXJyb3IudHh0IiwgInciKQogICAgZmlsZXM6d3JpdGUoZXJyb3IpCiAgICBmaWxlczpjbG9zZSgpCmVuZAoKbG9jYWwgc3RhdHVzLCBlcnIgPSBwY2FsbChtb2RpZnlfdGV4dHMpCmlmIG5vdCBzdGF0dXMgdGhlbgogICAgb25fZXJyb3IoZXJyKQplbmQ="));
                    break;
                case "CHT":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gbW9kaWZ5X3RleHRzKCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlcueCuumWiea6kOWFjeiyu+S8uuacjeWZqOi7n+mrlFxu5q2k5Ly65pyN5Zmo6Luf6auU5YOF55So5L2c5a2457+S5Lqk5rWB77yM6KuL5pSv5oyB5q2j54mI6YGK5oiyICAiIC4uIHVpZC50ZXh0CiAgICAgICAgdWlkLmZvbnRTaXplID0gNzYuMAogICAgZW5kCgogICAgbG9jYWwgYmV0YSA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVUlSb290L0Fib3ZlRGlhbG9nL0JldGFIaW50RGlhbG9nKENsb25lKSIpOkdldENvbXBvbmVudCgiVGV4dCIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gb25fZXJyb3IoZXJyb3IpCiAgICBDUy5Vbml0eUVuZ2luZS5BcHBsaWNhdGlvbi50YXJnZXRGcmFtZVJhdGUgPSAxMDAKICAgIENTLlVuaXR5RW5naW5lLlF1YWxpdHlTZXR0aW5ncy52U3luY0NvdW50ID0gMAogICAgbG9jYWwgZmlsZXMgPSBpby5vcGVuKCIuL2Vycm9yLnR4dCIsICJ3IikKICAgIGZpbGVzOndyaXRlKGVycm9yKQogICAgZmlsZXM6Y2xvc2UoKQplbmQKCmxvY2FsIHN0YXR1cywgZXJyID0gcGNhbGwobW9kaWZ5X3RleHRzKQppZiBub3Qgc3RhdHVzIHRoZW4KICAgIG9uX2Vycm9yKGVycikKZW5k"));
                    break;
                default:
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gbW9kaWZ5X3RleHRzKCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlciBpcyBhIGZyZWUgY2xvc2VkLXNvdXJjZSBzb2Z0d2FyZVxuRWR1Y2F0aW9uYWwgcHVycG9zZSBvbmx5LCBwbGVhc2Ugc3VwcG9ydCBnZW51aW5lIGdhbWUgIiAuLiB1aWQudGV4dAogICAgICAgIHVpZC5mb250U2l6ZSA9IDc2LjAKICAgIGVuZAoKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCmxvY2FsIGZ1bmN0aW9uIG9uX2Vycm9yKGVycm9yKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24udGFyZ2V0RnJhbWVSYXRlID0gMTAwCiAgICBDUy5Vbml0eUVuZ2luZS5RdWFsaXR5U2V0dGluZ3MudlN5bmNDb3VudCA9IDAKICAgIGxvY2FsIGZpbGVzID0gaW8ub3BlbigiLi9lcnJvci50eHQiLCAidyIpCiAgICBmaWxlczp3cml0ZShlcnJvcikKICAgIGZpbGVzOmNsb3NlKCkKZW5kCgpsb2NhbCBzdGF0dXMsIGVyciA9IHBjYWxsKG1vZGlmeV90ZXh0cykKaWYgbm90IHN0YXR1cyB0aGVuCiAgICBvbl9lcnJvcihlcnIpCmVuZA=="));
                    break;
            }

            await SendPacket(lData.BuildPacket());
        }

        if (packet.CmdId == CmdIds.GetTutorialScRsp)
        {
            BasePacket lData;
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9zci5taWhveW8uY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAi5qyi6L+O5p2l5YiwIERhbmhlbmdTZXJ2ZXIhXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5q2k5pyN5Yqh56uv5a6M5YWo5YWN6LS5XG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5aaC5p6c5L2g6YCa6L+H5LuY6LS55b6X5Yiw77yM6YKj5LmI5L2g5bey57uP6KKr6aqX5LqG44CCXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5pyN5Yqh56uv5LuF55So5LqO5a2m5Lmg5Lqk5rWB77yM6K+35pSv5oyB5q2j54mI5ri45oiPIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                    break;
                case "CHT":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9zci5taWhveW8uY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAi5q2h6L+O5L6G5YiwIERhbmhlbmdTZXJ2ZXIhXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5q2k5pyN5YuZ56uv5a6M5YWo5YWN6LK7XG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5aaC5p6c5L2g6YCa6YGO5LuY6LK75b6X5Yiw77yM6YKj6bq95L2g5bey57aT6KKr6aiZ556t44CCXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5pyN5YuZ56uv5YOF55So5pa85a2457+S5Lqk5rWB77yM6KuL5pSv5oyB5q2j54mI5ri45oiyIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                    break;
                default:
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIldlbGNvbWUgdG8gRGFuaGVuZ1NlcnZlciFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICJUaGlzIHNlcnZlciBzb2Z0d2FyZSBpcyB0b3RhbGx5IGZyZWUuXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAiSWYgeW91IHBheSBmb3IgaXQsIHlvdSBoYXZlIGJlZW4gc2NhbW1lZC5cbiIKICAgIHRleHQgPSB0ZXh0IC4uICJFZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                    break;
            }

            await SendPacket(lData.BuildPacket());
        }
    }

    public async Task SendPacket(int cmdId)
    {
        await SendPacket(new BasePacket((ushort)cmdId));
    }
}
