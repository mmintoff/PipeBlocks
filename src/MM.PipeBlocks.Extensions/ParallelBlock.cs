using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes multiple blocks in parallel and joins their results.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
public class ParallelBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly IBlock<V>[] _blocks;
    private readonly Either<Join<V>, JoinAsync<V>> _joiner;
    private readonly Clone<V> _cloner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelBlock{V}"/> class.
    /// </summary>
    /// <param name="blocks">The blocks to execute in parallel.</param>
    /// <param name="joiner">The joiner function to combine the results of the parallel blocks.</param>
    /// <param name="cloner">A function to clone the parameter.</param>
    public ParallelBlock(
        IBlock<V>[] blocks,
        Either<Join<V>, JoinAsync<V>> joiner,
        Clone<V>? cloner = null)
        => (_blocks, _joiner, _cloner) = (blocks, joiner, cloner ?? (static v => default));

    /// <summary>
    /// Executes the blocks synchronously in parallel and then joins the results.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <returns>The modified parameter after executing and joining the results.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        Parameter<V>[] values = [.. Enumerable.Range(0, _blocks.Length).Select(_ => Clone(value))];

        Parallel.ForEach(_blocks, (block, state, index) =>
        {
            values[index] = BlockExecutor.ExecuteSync(block, values[index]);
        });

        return _joiner.Match(
            joiner => joiner(value, values),
            joiner => AsyncContext.Run(async () => await joiner(value, values)));
    }

    /// <summary>
    /// Executes the blocks asynchronously in parallel and then joins the results.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the modified parameter after execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        var tasks = new Task<Parameter<V>>[_blocks.Length];

        for (int i = 0; i < _blocks.Length; i++)
        {
            tasks[i] = BlockExecutor.ExecuteAsync(_blocks[i], Clone(value)).AsTask();
        }

        var result = await Task.WhenAll(tasks);

        return await _joiner.MatchAsync(
            joiner => ValueTask.FromResult(joiner(value, result)),
            async joiner => await joiner(value, result));
    }

    private Parameter<V> Clone(Parameter<V> value)
        => value.Clone(_cloner(value));
}

/// <summary>
/// Extension methods for creating parallelized blocks.
/// </summary>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates a new <see cref="ParallelBlock{V}"/> that executes multiple blocks in parallel.
    /// </summary>
    /// <typeparam name="V">The value type associated with the parameter.</typeparam>
    /// <param name="_">The block builder used to create the parallel block.</param>
    /// <param name="blocks">The blocks to execute in parallel.</param>
    /// <param name="joiner">The function to join the results of the parallel blocks.</param>
    /// <param name="cloner">The function used to clone the parameter.</param>
    /// <returns>A new <see cref="ParallelBlock{V}"/> instance.</returns>
    public static ParallelBlock<V> Parallelize<V>(this BlockBuilder<V> _,
        IBlock<V>[] blocks,
        Either<Join<V>, JoinAsync<V>> joiner,
        Clone<V>? cloner = null)
        => new(blocks, joiner, cloner);
}

/// <summary>
/// A delegate that defines a method for cloning a parameter.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
/// <param name="value">The parameter to clone.</param>
/// <returns>The cloned parameter.</returns>
public delegate V? Clone<V>(Parameter<V> value);

/// <summary>
/// An asynchronous delegate that defines a method for cloning a parameter.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
/// <param name="value">The parameter to clone.</param>
/// <returns>A task representing the asynchronous operation to clone the parameter.</returns>
public delegate ValueTask<Parameter<V>> CloneAsync<V>(Parameter<V> value);

/// <summary>
/// A delegate that defines a method to join the results of parallel blocks into a single parameter.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
/// <param name="originalValue">The original parameter before parallel execution.</param>
/// <param name="parallelValues">The parameters from each parallel block execution.</param>
/// <returns>The joined parameter after combining the parallel parameters.</returns>
public delegate Parameter<V> Join<V>(Parameter<V> originalValue, Parameter<V>[] parallelValues);

/// <summary>
/// An asynchronous delegate that defines a method to join the results of parallel blocks into a single parameter.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
/// <param name="originalValue">The original parameter before parallel execution.</param>
/// <param name="parallelValues">The parameters from each parallel block execution.</param>
/// <returns>A task representing the asynchronous operation to join the parameters into a single parameter.</returns>
public delegate ValueTask<Parameter<V>> JoinAsync<V>(Parameter<V> originalValue, Parameter<V>[] parallelValues);