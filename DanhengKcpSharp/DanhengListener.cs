using System.Net;
using System.Net.Sockets;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Kcp.KcpSharp;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.Kcp;

public class DanhengListener
{
    private static UdpClient? UDPClient;
    private static IPEndPoint? ListenAddress;
    private static IKcpTransport<IKcpMultiplexConnection>? KCPTransport;
    private static readonly Logger Logger = new("GameServer");
    public static readonly SortedList<long, DanhengConnection> Connections = [];

    private static readonly KcpConversationOptions ConvOpt = new()
    {
        StreamMode = false,
        Mtu = 1400,
        ReceiveWindow = 256,
        SendWindow = 256,
        NoDelay = true,
        UpdateInterval = 100,
        KeepAliveOptions = new KcpKeepAliveOptions(1000, 30000)
    };

    public static Type BaseConnection { get; set; } = typeof(DanhengConnection);

    private static Socket? UDPListener => UDPClient?.Client;
    private static IKcpMultiplexConnection? Multiplex => KCPTransport?.Connection;
    private static uint PORT => ConfigManager.Config.GameServer.Port;

    public static DanhengConnection? GetConnectionByEndPoint(IPEndPoint ep)
    {
        return Connections.Values.FirstOrDefault(c => c.RemoteEndPoint.Equals(ep));
    }

    public static void StartListener()
    {
        ListenAddress = new IPEndPoint(IPAddress.Parse(ConfigManager.Config.GameServer.BindAddress), (int)PORT);
        UDPClient = new UdpClient(ListenAddress);
        if (UDPListener == null) return;
        KCPTransport = KcpSocketTransport.CreateMultiplexConnection(UDPClient, 1400);
        KCPTransport.Start();
        Logger.Info(I18NManager.Translate("Server.ServerInfo.ServerRunning", I18NManager.Translate("Word.Game"),
            ConfigManager.Config.GameServer.GetDisplayAddress()));
    }

    private static void RegisterConnection(DanhengConnection con)
    {
        if (!con.ConversationId.HasValue) return;
        Connections[con.ConversationId.Value] = con;
    }

    public static void UnregisterConnection(DanhengConnection con)
    {
        if (!con.ConversationId.HasValue) return;
        var convId = con.ConversationId.Value;
        if (Connections.Remove(convId))
        {
            Multiplex?.UnregisterConversation(convId);
            Logger.Info($"Connection with {con.RemoteEndPoint} has been closed");
        }
    }

    public static async Task HandleHandshake(UdpReceiveResult rcv)
    {
        try
        {
            var con = GetConnectionByEndPoint(rcv.RemoteEndPoint);
            await using MemoryStream? ms = new(rcv.Buffer);
            using BinaryReader? br = new(ms);
            var code = br.ReadInt32BE();
            br.ReadUInt32();
            br.ReadUInt32();
            var enet = br.ReadInt32BE();
            br.ReadUInt32();
            switch (code)
            {
                case 0x000000FF:
                    if (con != null)
                    {
                        Logger.Info($"Duplicate handshake from {con.RemoteEndPoint}");
                        return;
                    }

                    await AcceptConnection(rcv, enet);
                    break;
                case 0x00000194:
                    if (con == null)
                    {
                        Logger.Info($"Inexistent connection asked for disconnect from {rcv.RemoteEndPoint}");
                        return;
                    }

                    await SendDisconnectPacket(con, 5);
                    break;
                case -934149376:
                    if (con != null)
                    {
                        Logger.Info($"Duplicate handshake from {con.RemoteEndPoint}");
                        return;
                    }

                    await AcceptConnection(rcv, enet);
                    break;
                default:
                    Logger.Error($"Invalid handshake code received {code}");
                    return;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to handle handshake: {ex}");
        }
    }

    private static async Task AcceptConnection(UdpReceiveResult rcv, int enet)
    {
        var convId = Connections.GetNextAvailableIndex();
        var convo = Multiplex?.CreateConversation(convId, rcv.RemoteEndPoint, ConvOpt);
        if (convo == null) return;
        var con = (DanhengConnection)Activator.CreateInstance(BaseConnection, convo, rcv.RemoteEndPoint)!;
        RegisterConnection(con);
        await SendHandshakeResponse(con, enet);
    }

    private static async Task SendHandshakeResponse(DanhengConnection user, int enet)
    {
        if (user == null || UDPClient == null || !user.ConversationId.HasValue) return;
        var convId = user.ConversationId.Value;
        await using MemoryStream? ms = new();
        await using BinaryWriter? bw = new(ms);
        bw.WriteInt32BE(0x00000145);
        bw.WriteConvID(convId);
        bw.WriteInt32BE(enet);
        bw.WriteInt32BE(0x14514545);
        var data = ms.ToArray();
        await UDPClient.SendAsync(data, data.Length, user.RemoteEndPoint);
    }

    public static async Task SendDisconnectPacket(DanhengConnection user, int code)
    {
        if (user == null || UDPClient == null || !user.ConversationId.HasValue) return;
        var convId = user.ConversationId.Value;
        await using MemoryStream? ms = new();
        await using BinaryWriter? bw = new(ms);
        bw.WriteInt32BE(0x00000194);
        bw.WriteConvID(convId);
        bw.WriteInt32BE(code);
        bw.WriteInt32BE(0x19419494);
        var data = ms.ToArray();
        await UDPClient.SendAsync(data, data.Length, user.RemoteEndPoint);
    }
}