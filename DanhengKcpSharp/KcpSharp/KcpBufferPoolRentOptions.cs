namespace EggLink.DanhengServer.Kcp.KcpSharp;

/// <summary>
///     The options to use when renting buffers from the pool.
/// </summary>
public readonly struct KcpBufferPoolRentOptions : IEquatable<KcpBufferPoolRentOptions>
{
    /// <summary>
    ///     The minimum size of the buffer.
    /// </summary>
    public int Size { get; }

    /// <summary>
    ///     True if the buffer may be passed to the outside of KcpSharp. False if the buffer is only used internally in
    ///     KcpSharp.
    /// </summary>
    public bool IsOutbound { get; }

    /// <summary>
    ///     Create a <see cref="KcpBufferPoolRentOptions" /> with the specified parameters.
    /// </summary>
    /// <param name="size">The minimum size of the buffer.</param>
    /// <param name="isOutbound">
    ///     True if the buffer may be passed to the outside of KcpSharp. False if the buffer is only used
    ///     internally in KcpSharp.
    /// </param>
    public KcpBufferPoolRentOptions(int size, bool isOutbound)
    {
        Size = size;
        IsOutbound = isOutbound;
    }

    /// <inheritdoc />
    public bool Equals(KcpBufferPoolRentOptions other)
    {
        return Size == other.Size && IsOutbound == other.IsOutbound;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is KcpBufferPoolRentOptions other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Size, IsOutbound);
    }
}