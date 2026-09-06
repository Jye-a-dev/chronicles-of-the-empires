using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Coordinates asynchronous pacing and tween synchronization on Godot's Main Thread.
/// Governed by a CancellationTokenSource to guarantee safe cancellation when changing scenes.
/// </summary>
public sealed class AsyncPacer : IDisposable
{
    private CancellationTokenSource _cts = new();

    public CancellationToken Token => _cts.Token;
    public bool IsCancellationRequested => _cts.IsCancellationRequested;

    /// <summary>
    /// Waits for a specified duration on the SceneTree timer with cancellation support.
    /// Throws OperationCanceledException immediately if cancellation is triggered.
    /// </summary>
    public async Task DelayAsync(SceneTree tree, float seconds, CancellationToken externalToken = default)
    {
        if (seconds <= 0f) return;

        using var linkedCts = externalToken == default
            ? null
            : CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, externalToken);

        var token = linkedCts?.Token ?? _cts.Token;
        token.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<bool>();
        var timer = tree.CreateTimer(seconds);

        void OnTimeout() => tcs.TrySetResult(true);
        timer.Timeout += OnTimeout;

        await using (token.Register(() => tcs.TrySetCanceled(token)))
        {
            try
            {
                await tcs.Task;
            }
            finally
            {
                timer.Timeout -= OnTimeout;
            }
        }
    }

    /// <summary>
    /// Cancels all ongoing pacing waits and prepares a fresh token.
    /// </summary>
    public void Cancel()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        _cts.Dispose();
    }
}

