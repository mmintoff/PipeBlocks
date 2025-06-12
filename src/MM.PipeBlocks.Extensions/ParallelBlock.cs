using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes multiple blocks in parallel and joins their results.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public class ParallelBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly IBlock<C, V>[] _blocks;
    private readonly Either<Join<C, V>, JoinAsync<C, V>> _joiner;
    private readonly Clone<C, V> _cloner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelBlock{C, V}"/> class.
    /// </summary>
    /// <param name="blocks">The blocks to execute in parallel.</param>
    /// <param name="joiner">The joiner function to combine the results of the parallel blocks.</param>
    /// <param name="cloner">A function to clone the context.</param>
    public ParallelBlock(
        IBlock<C, V>[] blocks,
        Either<Join<C, V>, JoinAsync<C, V>> joiner,
        Clone<C, V> cloner)
        => (_blocks, _joiner, _cloner) = (blocks, joiner, cloner);

    /// <summary>
    /// Executes the blocks synchronously in parallel and then joins the results.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>The modified context after executing and joining the results.</returns>
    public C Execute(C context)
    {
        var clonedContexts = CloneContexts(context);
        Parallel.ForEach(_blocks, (block, state, index) =>
            clonedContexts[index] = BlockExecutor.ExecuteSync(block, clonedContexts[index]));

        return _joiner.Match(
            joiner => joiner(context, clonedContexts),
            joiner => AsyncContext.Run(async () => await joiner(context, clonedContexts)));
    }

    /// <summary>
    /// Executes the blocks asynchronously in parallel and then joins the results.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the modified context after execution.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        var clonedContexts = CloneContexts(context);
        var tasks = new Task<C>[_blocks.Length];
        for (int i = 0; i < _blocks.Length; i++)
            tasks[i] = BlockExecutor.ExecuteAsync(_blocks[i], clonedContexts[i]).AsTask();

        var result = await Task.WhenAll(tasks);

        return await _joiner.MatchAsync(
            joiner => ValueTask.FromResult(joiner(context, result)),
            async joiner => await joiner(context, result));
    }

    private C[] CloneContexts(C context) => [.. _blocks.Select(_ =>
    {
        var clone = _cloner(context);
        clone.IsFlipped = context.IsFlipped;
        return clone;
    })];
}

/// <summary>
/// Extension methods for creating parallelized blocks.
/// </summary>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates a new <see cref="ParallelBlock{C, V}"/> that executes multiple blocks in parallel.
    /// </summary>
    /// <typeparam name="C">The context type.</typeparam>
    /// <typeparam name="V">The value type associated with the context.</typeparam>
    /// <param name="_">The block builder used to create the parallel block.</param>
    /// <param name="blocks">The blocks to execute in parallel.</param>
    /// <param name="joiner">The function to join the results of the parallel blocks.</param>
    /// <param name="cloner">The function used to clone the context.</param>
    /// <returns>A new <see cref="ParallelBlock{C, V}"/> instance.</returns>
    public static ParallelBlock<C, V> Parallelize<C, V>(this BlockBuilder<C, V> _,
        IBlock<C, V>[] blocks,
        Clone<C, V> cloner,
        Either<Join<C, V>, JoinAsync<C, V>> joiner)
        where C : IContext<V>
        => new(blocks, joiner, cloner);
}

/// <summary>
/// A delegate that defines a method for cloning a context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="context">The context to clone.</param>
/// <returns>The cloned context.</returns>
public delegate C Clone<C, V>(C context)
    where C : IContext<V>
    ;

/// <summary>
/// An asynchronous delegate that defines a method for cloning a context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="context">The context to clone.</param>
/// <returns>A task representing the asynchronous operation to clone the context.</returns>
public delegate ValueTask<C> CloneAsync<C, V>(C context)
    where C : IContext<V>
    ;

/// <summary>
/// A delegate that defines a method to join the results of parallel blocks into a single context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="originalContext">The original context before parallel execution.</param>
/// <param name="parallelContexts">The contexts from each parallel block execution.</param>
/// <returns>The joined context after combining the parallel contexts.</returns>
public delegate C Join<C, V>(C originalContext, C[] parallelContexts)
    where C : IContext<V>
    ;

/// <summary>
/// An asynchronous delegate that defines a method to join the results of parallel blocks into a single context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="originalContext">The original context before parallel execution.</param>
/// <param name="parallelContexts">The contexts from each parallel block execution.</param>
/// <returns>A task representing the asynchronous operation to join the contexts into a single context.</returns>
public delegate ValueTask<C> JoinAsync<C, V>(C originalContext, C[] parallelContexts)
    where C : IContext<V>
    ;
