using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that adapts parameters between two different value types, executes a series of blocks in the adapted parameter,
/// and then adapts the result back to the original value type.
/// </summary>
/// <typeparam name="V1">The original value type associated with the parameter.</typeparam>
/// <typeparam name="V2">The adapted value type associated with the parameter.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AdapterPipeBlock{V1, V2}"/> class.
/// </remarks>
/// <param name="pipeName">The name of the pipe.</param>
/// <param name="adapter">The adapter used to convert parameters between <typeparamref name="V1"/> and <typeparamref name="V2"/>.</param>
/// <param name="blockBuilder">The block builder used to resolve blocks.</param>
public sealed class AdapterPipeBlock<V1, V2>(
        string pipeName,
        IAdapter<V1, V2> adapter,
        BlockBuilder<V2> blockBuilder
        ) : ISyncBlock<V1>, IAsyncBlock<V1>
{
    private readonly List<IBlock<V2>> _blocks = [];
    private readonly ILogger<AdapterPipeBlock<V1, V2>> _logger = blockBuilder.CreateLogger<AdapterPipeBlock<V1, V2>>();

    /// <summary>
    /// Executes the block synchronously, switching between parameters as necessary.
    /// </summary>
    /// <param name="value">The original parameter to execute.</param>
    /// <returns>The original parameter after execution, potentially modified.</returns>
    public Parameter<V1> Execute(Parameter<V1> value)
    {
        _logger.LogTrace("Switching value from: {V1} to: {V2}", typeof(V1).Name, typeof(V2).Name);
        var nContext = adapter.Adapt(value);
        _logger.LogTrace("Executing pipe: '{name}' synchronously for context: {CorrelationId}", pipeName, value.Context.CorrelationId);
        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(nContext))
            {
                _logger.LogTrace("Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, value.Context.CorrelationId);
                break;
            }

            nContext = BlockExecutor.ExecuteSync(_blocks[i], nContext);
        }
        _logger.LogTrace("Completed synchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, value.Context.CorrelationId);
        _logger.LogTrace("Switching value from: {V2} to: {V1}", typeof(V2).Name, typeof(V1).Name);
        return adapter.Adapt(nContext);
    }

    /// <summary>
    /// Executes the block asynchronously, switching between parameters as necessary.
    /// </summary>
    /// <param name="value">The original parameter to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the original parameter potentially modified.</returns>
    public async ValueTask<Parameter<V1>> ExecuteAsync(Parameter<V1> value)
    {
        _logger.LogTrace("Switching value from: {V1} to: {V2}", typeof(V1).Name, typeof(V2).Name);
        var nContext = adapter.Adapt(value);
        _logger.LogTrace("Executing pipe: '{name}' asynchronously for context: {CorrelationId}", pipeName, value.CorrelationId);
        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(nContext))
            {
                _logger.LogTrace("Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, value.CorrelationId);
                break;
            }

            nContext = await BlockExecutor.ExecuteAsync(_blocks[i], nContext);
        }
        _logger.LogTrace("Switching value from: {V2} to: {V1}", typeof(V2).Name, typeof(V1).Name);
        _logger.LogTrace("Completed synchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, value.CorrelationId);
        return adapter.Adapt(nContext);
    }

    /// <summary>
    /// Converts the block to a function that executes it synchronously.
    /// </summary>
    /// <returns>A function that executes the block synchronously.</returns>
    public Func<Parameter<V1>, Parameter<V1>> ToFunc() => Execute;

    /// <summary>
    /// Converts the block to a function that executes it asynchronously.
    /// </summary>
    /// <returns>A function that executes the block asynchronously.</returns>
    public Func<Parameter<V1>, ValueTask<Parameter<V1>>> ToAsyncFunc() => ExecuteAsync;

    /// <summary>
    /// Adds a block to the pipe to be executed after the current one.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current <see cref="AdapterPipeBlock{V1, V2}"/> instance.</returns>
    public AdapterPipeBlock<V1, V2> Then(IBlock<V2> block)
        => AddBlock(block);

    /// <summary>
    /// Adds a block to the pipe to be executed after the current one.
    /// </summary>
    /// <typeparam name="X">The type of the block to add.</typeparam>
    /// <returns>The current <see cref="AdapterPipeBlock{V1, V2}"/> instance.</returns>
    public AdapterPipeBlock<V1, V2> Then<X>()
        where X : IBlock<V2>
        => AddBlock(blockBuilder.ResolveInstance<X>());

    /// <summary>
    /// Adds a block to the pipe to be executed after the current one.
    /// </summary>
    /// <param name="func">A function that resolves the next block.</param>
    /// <returns>The current <see cref="AdapterPipeBlock{V1, V2}"/> instance.</returns>
    public AdapterPipeBlock<V1, V2> Then(Func<BlockBuilder<V2>, IBlock<V2>> func)
        => AddBlock(func(blockBuilder));

    private AdapterPipeBlock<V1, V2> AddBlock(IBlock<V2> block)
    {
        _blocks.Add(block);
        return this;
    }

    private static bool IsFinished(Parameter<V2> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || IsFailure(value))
        : value.Context.IsFinished || IsFailure(value);

    private static bool IsFailure(Parameter<V2> value) => value.Match(
        _ => true,
        _ => false);

    public override string ToString() => pipeName;
}

/// <summary>
/// Defines an adapter that can convert between two different parameter value types.
/// </summary>
/// <typeparam name="V1">The original parameter value type.</typeparam>
/// <typeparam name="V2">The adapted parameter value type.</typeparam>
public interface IAdapter<V1, V2>
{
    /// <summary>
    /// Adapts a parameter with value type <typeparamref name="V1"/> to value type <typeparamref name="V2"/>.
    /// </summary>
    /// <param name="from">The parameter with value type <typeparamref name="V1"/> to adapt.</param>
    /// <returns>The adapted parameter with value type <typeparamref name="V2"/>.</returns>
    Parameter<V2> Adapt(Parameter<V1> from);

    /// <summary>
    /// Adapts a parameter with value type <typeparamref name="V2"/> to value type <typeparamref name="V1"/>.
    /// </summary>
    /// <param name="from">The parameter with value type <typeparamref name="V2"/> to adapt.</param>
    /// <returns>The adapted parameter with value type <typeparamref name="V1"/>.</returns>
    Parameter<V1> Adapt(Parameter<V2> from);
}

/// <summary>
/// Extension method to create a new <see cref="AdapterPipeBlock{V1, V2}"/> with the specified parameters.
/// </summary>
/// <returns>A new instance of <see cref="AdapterPipeBlock{V1, V2}"/>, which represents a pipeline that adapts between the two value types and can execute a series of blocks.</returns>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates an <see cref="AdapterPipeBlock{V1, V2}"/> pipeline, starting with the provided <paramref name="pipeName"/>, 
    /// and setting up the adapter and block builder for transforming and processing the parameters.
    /// </summary>
    /// <param name="_">The block builder for resolving the blocks.</param>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="builder">The block builder for resolving the blocks.</param>
    /// <param name="adapter">The adapter responsible for transforming between source and target parameters.</param>
    /// <typeparam name="V1">The value type associated with the source parameter.</typeparam>
    /// <typeparam name="V2">The value type associated with the target parameter.</typeparam>
    /// <returns>A new instance of <see cref="AdapterPipeBlock{V1, V2}"/> configured with the provided parameters.</returns>
    public static AdapterPipeBlock<V1, V2> CreatePipe<V1, V2>(this BlockBuilder<V1> _,
        string pipeName,
        BlockBuilder<V2> builder,
        IAdapter<V1, V2> adapter)
    => new(pipeName, adapter, builder);
}