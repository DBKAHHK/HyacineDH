namespace EggLink.DanhengServer.Kcp.KcpSharp;

internal struct KcpSendReceiveBufferItem
{
    public KcpBuffer Data;
    public KcpPacketHeader Segment;
    public KcpSendSegmentStats Stats;
}