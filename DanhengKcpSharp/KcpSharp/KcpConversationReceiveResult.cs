using System.Globalization;

namespace EggLink.DanhengServer.Kcp.KcpSharp;

/// <summary>
///     The result of a receive or peek operation.
/// </summary>
public readonly struct KcpConversationReceiveResult : IEquatable<KcpConversationReceiveResult>
{
    private readonly bool _connectionAlive;

    /// <summary>
    ///     The number of bytes received.
    /// </summary>
    public int BytesReceived { get; }

    /// <summary>
    ///     Whether the underlying transport is marked as closed.
    /// </summary>
    public bool TransportClosed => !_connectionAlive;

    /// <summary>
    ///     Construct a <see cref="KcpConversationReceiveResult" /> with the specified number of bytes received.
    /// </summary>
    /// <param name="bytesReceived">The number of bytes received.</param>
    public KcpConversationReceiveResult(int bytesReceived)
    {
        BytesReceived = bytesReceived;
        _connectionAlive = true;
    }

    /// <summary>
    ///     Checks whether the two instance is equal.
    /// </summary>
    /// <param name="left">The one instance.</param>
    /// <param name="right">The other instance.</param>
    /// <returns>Whether the two instance is equal</returns>
    public static bool operator ==(KcpConversationReceiveResult left, KcpConversationReceiveResult right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks whether the two instance is not equal.
    /// </summary>
    /// <param name="left">The one instance.</param>
    /// <param name="right">The other instance.</param>
    /// <returns>Whether the two instance is not equal</returns>
    public static bool operator !=(KcpConversationReceiveResult left, KcpConversationReceiveResult right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(KcpConversationReceiveResult other)
    {
        return BytesReceived == other.BytesReceived && TransportClosed == other.TransportClosed;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is KcpConversationReceiveResult other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(BytesReceived, TransportClosed);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _connectionAlive ? BytesReceived.ToString(CultureInfo.InvariantCulture) : "Transport is closed.";
    }
}