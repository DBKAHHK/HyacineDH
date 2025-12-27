using System.Diagnostics;
using System.Threading.Tasks.Sources;

namespace EggLink.DanhengServer.Kcp.KcpSharp;

internal class AsyncAutoResetEvent<T> : IValueTaskSource<T>
{
    private bool _activeWait;
    private bool _isSet;
    private SpinLock _lock;
    private ManualResetValueTaskSourceCore<T> _rvtsc;
    private bool _signaled;

    private T? _value;

    public AsyncAutoResetEvent()
    {
        _rvtsc = new ManualResetValueTaskSourceCore<T>
        {
            RunContinuationsAsynchronously = true
        };
        _lock = new SpinLock();
    }

    T IValueTaskSource<T>.GetResult(short token)
    {
        try
        {
            return _rvtsc.GetResult(token);
        }
        finally
        {
            _rvtsc.Reset();

            var lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                _activeWait = false;
                _signaled = false;
            }
            finally
            {
                if (lockTaken) _lock.Exit();
            }
        }
    }

    ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
    {
        return _rvtsc.GetStatus(token);
    }

    void IValueTaskSource<T>.OnCompleted(Action<object?> continuation, object? state, short token,
        ValueTaskSourceOnCompletedFlags flags)
    {
        _rvtsc.OnCompleted(continuation, state, token, flags);
    }

    public ValueTask<T> WaitAsync()
    {
        var lockTaken = false;
        try
        {
            _lock.Enter(ref lockTaken);

            if (_activeWait)
                return new ValueTask<T>(
                    Task.FromException<T>(new InvalidOperationException("Another thread is already waiting.")));
            if (_isSet)
            {
                _isSet = false;
                var value = _value!;
                _value = default;
                return new ValueTask<T>(value);
            }

            _activeWait = true;
            Debug.Assert(!_signaled);

            return new ValueTask<T>(this, _rvtsc.Version);
        }
        finally
        {
            if (lockTaken) _lock.Exit();
        }
    }

    public void Set(T value)
    {
        var lockTaken = false;
        try
        {
            _lock.Enter(ref lockTaken);

            if (_activeWait && !_signaled)
            {
                _signaled = true;
                _rvtsc.SetResult(value);
                return;
            }

            _isSet = true;
            _value = value;
        }
        finally
        {
            if (lockTaken) _lock.Exit();
        }
    }
}