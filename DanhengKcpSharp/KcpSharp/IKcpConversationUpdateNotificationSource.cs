namespace EggLink.DanhengServer.Kcp.KcpSharp;

internal interface IKcpConversationUpdateNotificationSource
{
    ReadOnlyMemory<byte> Packet { get; }
    void Release();
}