using System.Diagnostics;

namespace EggLink.DanhengServer.Kcp.KcpSharp;

internal readonly struct KcpBuffer
{
    private readonly object? _owner;
    private readonly Memory<byte> _memory;

    public ReadOnlyMemory<byte> DataRegion => _memory.Slice(0, Length);

    public int Length { get; }

    private KcpBuffer(object? owner, Memory<byte> memory, int length)
    {
        _owner = owner;
        _memory = memory;
        Length = length;
    }

    public static KcpBuffer CreateFromSpan(KcpRentedBuffer buffer, ReadOnlySpan<byte> dataSource)
    {
        var memory = buffer.Memory;
        if (dataSource.Length > memory.Length) ThrowRentedBufferTooSmall();
        dataSource.CopyTo(memory.Span);
        return new KcpBuffer(buffer.Owner, memory, dataSource.Length);
    }

    public KcpBuffer AppendData(ReadOnlySpan<byte> data)
    {
        if (Length + data.Length > _memory.Length) ThrowRentedBufferTooSmall();
        data.CopyTo(_memory.Span.Slice(Length));
        return new KcpBuffer(_owner, _memory, Length + data.Length);
    }

    public KcpBuffer Consume(int length)
    {
        Debug.Assert((uint)length <= (uint)Length);
        return new KcpBuffer(_owner, _memory.Slice(length), Length - length);
    }

    public void Release()
    {
        new KcpRentedBuffer(_owner, _memory).Dispose();
    }

    private static void ThrowRentedBufferTooSmall()
    {
        throw new InvalidOperationException("The rented buffer is not large enough to hold the data.");
    }
}