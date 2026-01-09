using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;

public class BlockExecutorTests
{
    // ---------- Test doubles ----------

    private sealed class SyncBlock<TIn, TOut>(Func<Parameter<TIn>, Parameter<TOut>> execute) : ISyncBlock<TIn, TOut>, IBlock<TIn, TOut>
    {
        private readonly Func<Parameter<TIn>, Parameter<TOut>> _execute = execute;

        public Parameter<TOut> Execute(Parameter<TIn> value) => _execute(value);
    }

    private sealed class AsyncBlock<TIn, TOut>(Func<Parameter<TIn>, ValueTask<Parameter<TOut>>> execute) : IAsyncBlock<TIn, TOut>, IBlock<TIn, TOut>
    {
        private readonly Func<Parameter<TIn>, ValueTask<Parameter<TOut>>> _execute = execute;

        public ValueTask<Parameter<TOut>> ExecuteAsync(Parameter<TIn> value)
            => _execute(value);
    }

    private sealed class BothBlock<TIn, TOut> :
        ISyncBlock<TIn, TOut>,
        IAsyncBlock<TIn, TOut>,
        IBlock<TIn, TOut>
    {
        public bool SyncCalled { get; private set; }
        public bool AsyncCalled { get; private set; }

        public Parameter<TOut> Execute(Parameter<TIn> value)
        {
            SyncCalled = true;
            return new Parameter<TOut>(left: default!);
        }

        public ValueTask<Parameter<TOut>> ExecuteAsync(Parameter<TIn> value)
        {
            AsyncCalled = true;
            return ValueTask.FromResult(new Parameter<TOut>(left: default!));
        }
    }

    // ---------- ExecuteSync tests ----------

    [Fact]
    public void ExecuteSync_UsesSyncBlock()
    {
        var block = new SyncBlock<int, int>(p => new Parameter<int>(p.Value + 1));
        var input = new Parameter<int>(1);

        var result = BlockExecutor.ExecuteSync(block, input);

        Assert.Equal(2, result.Value);
    }

    [Fact]
    public void ExecuteSync_UsesAsyncBlockSynchronously()
    {
        var block = new AsyncBlock<int, int>(p =>
            new ValueTask<Parameter<int>>(new Parameter<int>(p.Value * 2)));

        var input = new Parameter<int>(2);

        var result = BlockExecutor.ExecuteSync(block, input);

        Assert.Equal(4, result.Value);
    }

    [Fact]
    public void ExecuteSync_PrefersSync_WhenBothImplemented()
    {
        var block = new BothBlock<int, int>();
        var input = new Parameter<int>(0);

        BlockExecutor.ExecuteSync(block, input);

        Assert.True(block.SyncCalled);
        Assert.False(block.AsyncCalled);
    }

    [Fact]
    public void ExecuteSync_Exception_NotHandled_Throws()
    {
        var block = new SyncBlock<int, int>(_ => throw new InvalidOperationException());
        var input = new Parameter<int>(1);

        Assert.Throws<InvalidOperationException>(
            () => BlockExecutor.ExecuteSync(block, input, handleExceptions: false));
    }

    [Fact]
    public void ExecuteSync_Exception_Handled_SignalsFailure()
    {
        var block = new SyncBlock<int, int>(_ => throw new InvalidOperationException());
        var input = new Parameter<int>(1);

        var result = BlockExecutor.ExecuteSync(block, input, handleExceptions: true);

        Assert.True(result.IsFailure);
        Assert.IsType<ExceptionFailureState<int>>(result.Failure);
    }

    // ---------- ExecuteAsync tests ----------

    [Fact]
    public async Task ExecuteAsync_UsesAsyncBlock()
    {
        var block = new AsyncBlock<int, int>(p =>
            new ValueTask<Parameter<int>>(new Parameter<int>(p.Value + 10)));

        var input = new Parameter<int>(5);

        var result = await BlockExecutor.ExecuteAsync(block, input);

        Assert.Equal(15, result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_PrefersAsync_WhenBothImplemented()
    {
        var block = new BothBlock<int, int>();
        var input = new Parameter<int>(0);

        await BlockExecutor.ExecuteAsync(block, input);

        Assert.True(block.AsyncCalled);
        Assert.False(block.SyncCalled);
    }

    [Fact]
    public async Task ExecuteAsync_Exception_NotHandled_Throws()
    {
        var block = new AsyncBlock<int, int>(_ =>
            throw new InvalidOperationException());

        var input = new Parameter<int>(1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => BlockExecutor.ExecuteAsync(block, input, handleExceptions: false).AsTask());
    }

    [Fact]
    public async Task ExecuteAsync_Exception_Handled_SignalsFailure()
    {
        var block = new AsyncBlock<int, int>(_ =>
            throw new InvalidOperationException());

        var input = new Parameter<int>(1);

        var result = await BlockExecutor.ExecuteAsync(block, input, handleExceptions: true);

        Assert.True(result.IsFailure);
        Assert.IsType<ExceptionFailureState<int>>(result.Failure);
    }
}