using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes multiple blocks in parallel and joins their results.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public class ParallelBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly IBlock<V>[] _blocks;
    private readonly Either<Join<V>, JoinAsync<V>> _joiner;
    private readonly Clone<V> _cloner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelBlock{C, V}"/> class.
    /// </summary>
    /// <param name="blocks">The blocks to execute in parallel.</param>
    /// <param name="joiner">The joiner function to combine the results of the parallel blocks.</param>
    /// <param name="cloner">A function to clone the context.</param>
    public ParallelBlock(
        IBlock<V>[] blocks,
        Either<Join<V>, JoinAsync<V>> joiner,
        Clone<V> cloner)
        => (_blocks, _joiner, _cloner) = (blocks, joiner, cloner);

    /// <summary>
    /// Executes the blocks synchronously in parallel and then joins the results.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>The modified context after executing and joining the results.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        var snapshot = Context.Capture();
        var values = new Parameter<V>[_blocks.Length];

        Parallel.ForEach(_blocks, (block, state, index) =>
        {
            snapshot.Apply();
            var valueClone = _cloner(value);
            values[index] = BlockExecutor.ExecuteSync(block, valueClone);
        });

        return _joiner.Match(
            joiner => joiner(value, values),
            joiner => AsyncContext.Run(async () => await joiner(value, values)));
    }

    /// <summary>
    /// Executes the blocks asynchronously in parallel and then joins the results.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the modified context after execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        var snapshot = Context.Capture();
        var tasks = new Task<Parameter<V>>[_blocks.Length];
        for (int i = 0; i < _blocks.Length; i++)
        {
            snapshot.Apply();
            var valueClone = _cloner(value);
            tasks[i] = BlockExecutor.ExecuteAsync(_blocks[i], valueClone).AsTask();
        }

        var result = await Task.WhenAll(tasks);

        return await _joiner.MatchAsync(
            joiner => ValueTask.FromResult(joiner(value, result)),
            async joiner => await joiner(value, result));
    }
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
    public static ParallelBlock<V> Parallelize<C, V>(this BlockBuilder<V> _,
        IBlock<V>[] blocks,
        Clone<V> cloner,
        Either<Join<V>, JoinAsync<V>> joiner)
        => new(blocks, joiner, cloner);
}

/// <summary>
/// A delegate that defines a method for cloning a context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="context">The context to clone.</param>
/// <returns>The cloned context.</returns>
public delegate Parameter<V> Clone<V>(Parameter<V> value);

/// <summary>
/// An asynchronous delegate that defines a method for cloning a context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="context">The context to clone.</param>
/// <returns>A task representing the asynchronous operation to clone the context.</returns>
public delegate ValueTask<Parameter<V>> CloneAsync<V>(Parameter<V> value);

/// <summary>
/// A delegate that defines a method to join the results of parallel blocks into a single context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="originalContext">The original context before parallel execution.</param>
/// <param name="parallelContexts">The contexts from each parallel block execution.</param>
/// <returns>The joined context after combining the parallel contexts.</returns>
public delegate Parameter<V> Join<V>(Parameter<V> originalValue, Parameter<V>[] parallelValues);

/// <summary>
/// An asynchronous delegate that defines a method to join the results of parallel blocks into a single context.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <param name="originalContext">The original context before parallel execution.</param>
/// <param name="parallelContexts">The contexts from each parallel block execution.</param>
/// <returns>A task representing the asynchronous operation to join the contexts into a single context.</returns>
public delegate ValueTask<Parameter<V>> JoinAsync<V>(Parameter<V> originalValue, Parameter<V>[] parallelValues);