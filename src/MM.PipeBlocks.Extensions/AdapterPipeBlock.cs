//using Microsoft.Extensions.Logging;
//using MM.PipeBlocks.Abstractions;

//namespace MM.PipeBlocks.Extensions;
///// <summary>
///// A block that adapts contexts between two different types, executes a series of blocks in the adapted context,
///// and then adapts the result back to the original context type.
///// </summary>
///// <typeparam name="C1">The original context type.</typeparam>
///// <typeparam name="V1">The value type associated with the original context.</typeparam>
///// <typeparam name="C2">The adapted context type.</typeparam>
///// <typeparam name="V2">The value type associated with the adapted context.</typeparam>
///// <remarks>
///// Initializes a new instance of the <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> class.
///// </remarks>
///// <param name="pipeName">The name of the pipe.</param>
///// <param name="adapter">The adapter used to convert contexts between <typeparamref name="C1"/> and <typeparamref name="C2"/>.</param>
///// <param name="blockBuilder">The block builder used to resolve blocks.</param>
//public sealed class AdapterPipeBlock<V1, V2>(
//        string pipeName,
//        IAdapter<V1, V2> adapter,
//        BlockBuilder<V2> blockBuilder
//        ) : ISyncBlock<V1>, IAsyncBlock<V1>
//{
//    private readonly List<IBlock<V2>> _blocks = [];
//    private readonly ILogger<AdapterPipeBlock<V1, V2>> _logger = blockBuilder.CreateLogger<AdapterPipeBlock<V1, V2>>();

//    /// <summary>
//    /// Executes the block synchronously, switching between contexts as necessary.
//    /// </summary>
//    /// <param name="context">The original context to execute.</param>
//    /// <returns>The original context after execution, potentially modified.</returns>
//    public Parameter<V1> Execute(Parameter<V1> context)
//    {
//        _logger.LogTrace("Switching context from: {C1} to: {C2}", typeof(C1).Name, typeof(C2).Name);
//        V2 nContext = adapter.Adapt(context);
//        _logger.LogTrace("Executing pipe: '{name}' synchronously for context: {CorrelationId}", pipeName, Context.CorrelationId);
//        for (int i = 0; i < _blocks.Count; i++)
//        {
//            if (IsFinished(nContext))
//            {
//                _logger.LogTrace("Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, Context.CorrelationId);
//                break;
//            }

//            nContext = BlockExecutor.ExecuteSync(_blocks[i], nContext);
//        }
//        _logger.LogTrace("Completed synchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, Context.CorrelationId);
//        _logger.LogTrace("Switching context from: {C2} to: {C1}", typeof(C2).Name, typeof(C1).Name);
//        return adapter.Adapt(nContext);
//    }

//    /// <summary>
//    /// Executes the block asynchronously, switching between contexts as necessary.
//    /// </summary>
//    /// <param name="context">The original context to execute.</param>
//    /// <returns>A task representing the asynchronous operation, with the original context potentially modified.</returns>
//    public async ValueTask<C1> ExecuteAsync(C1 context)
//    {
//        _logger.LogTrace("Switching context from: {C1} to: {C2}", typeof(C1).Name, typeof(C2).Name);
//        C2 nContext = adapter.Adapt(context);
//        _logger.LogTrace("Executing pipe: '{name}' asynchronously for context: {CorrelationId}", pipeName, context.CorrelationId);
//        for (int i = 0; i < _blocks.Count; i++)
//        {
//            if (IsFinished(nContext))
//            {
//                _logger.LogTrace("Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, context.CorrelationId);
//                break;
//            }

//            nContext = await BlockExecutor.ExecuteAsync(_blocks[i], nContext);
//        }
//        _logger.LogTrace("Switching context from: {C2} to: {C1}", typeof(C2).Name, typeof(C1).Name);
//        _logger.LogTrace("Completed synchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, context.CorrelationId);
//        return adapter.Adapt(nContext);
//    }

//    /// <summary>
//    /// Converts the block to a function that executes it synchronously.
//    /// </summary>
//    /// <returns>A function that executes the block synchronously.</returns>
//    public Func<C1, C1> ToFunc() => Execute;

//    /// <summary>
//    /// Converts the block to a function that executes it asynchronously.
//    /// </summary>
//    /// <returns>A function that executes the block asynchronously.</returns>
//    public Func<C1, ValueTask<C1>> ToAsyncFunc() => ExecuteAsync;

//    /// <summary>
//    /// Adds a block to the pipe to be executed after the current one.
//    /// </summary>
//    /// <param name="block">The block to add.</param>
//    /// <returns>The current <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> instance.</returns>
//    public AdapterPipeBlock<C1, V1, C2, V2> Then(IBlock<C2, V2> block)
//        => AddBlock(block);

//    /// <summary>
//    /// Adds a block to the pipe to be executed after the current one.
//    /// </summary>
//    /// <typeparam name="X">The type of the block to add.</typeparam>
//    /// <returns>The current <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> instance.</returns>
//    public AdapterPipeBlock<C1, V1, C2, V2> Then<X>()
//        where X : IBlock<C2, V2>
//        => AddBlock(blockBuilder.ResolveInstance<X>());

//    /// <summary>
//    /// Adds a block to the pipe to be executed after the current one.
//    /// </summary>
//    /// <param name="func">A function that resolves the next block.</param>
//    /// <returns>The current <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> instance.</returns>
//    public AdapterPipeBlock<C1, V1, C2, V2> Then(Func<BlockBuilder<C2, V2>, IBlock<C2, V2>> func)
//        => AddBlock(func(blockBuilder));

//    private AdapterPipeBlock<C1, V1, C2, V2> AddBlock(IBlock<C2, V2> block)
//    {
//        _blocks.Add(block);
//        return this;
//    }

//    private static bool IsFinished(C2 c) => c.IsFlipped
//        ? !(c.IsFinished || IsFailure(c))
//        : c.IsFinished || IsFailure(c);

//    private static bool IsFailure(C2 c) => c.Value.Match(
//        _ => true,
//        _ => false);

//    public override string ToString() => pipeName;
//}

///// <summary>
///// Defines an adapter that can convert between two different context types.
///// </summary>
///// <typeparam name="C1">The original context type.</typeparam>
///// <typeparam name="V1">The value type associated with the original context.</typeparam>
///// <typeparam name="C2">The adapted context type.</typeparam>
///// <typeparam name="V2">The value type associated with the adapted context.</typeparam>
//public interface IAdapter<C1, V1, C2, V2>
//    where C1 : IContext<V1>
//    where C2 : IContext<V2>
//{
//    /// <summary>
//    /// Adapts a context of type <typeparamref name="C1"/> to <typeparamref name="C2"/>.
//    /// </summary>
//    /// <param name="from">The context of type <typeparamref name="C1"/> to adapt.</param>
//    /// <returns>The adapted context of type <typeparamref name="C2"/>.</returns>
//    C2 Adapt(C1 from);

//    /// <summary>
//    /// Adapts a context of type <typeparamref name="C2"/> to <typeparamref name="C1"/>.
//    /// </summary>
//    /// <param name="from">The context of type <typeparamref name="C2"/> to adapt.</param>
//    /// <returns>The adapted context of type <typeparamref name="C1"/>.</returns>
//    C1 Adapt(C2 from);
//}

///// <summary>
///// Extension method to create a new <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> with the specified parameters.
///// </summary>
///// <returns>A new instance of <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/>, which represents a pipeline that adapts between the two context types and can execute a series of blocks.</returns>
//public static partial class BuilderExtensions
//{
//    /// <summary>
//    /// Creates an <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> pipeline, starting with the provided <paramref name="pipeName"/>, 
//    /// and setting up the adapter and block builder for transforming and processing the contexts.
//    /// </summary>
//    /// <param name="_">The block builder for resolving the blocks.</param>
//    /// <param name="pipeName">The name of the pipe.</param>
//    /// <param name="builder">The block builder for resolving the blocks.</param>
//    /// <param name="adapter">The adapter responsible for transforming between source and target contexts.</param>
//    /// <typeparam name="C1">The source context type.</typeparam>
//    /// <typeparam name="V1">The value type associated with the source context.</typeparam>
//    /// <typeparam name="C2">The target context type.</typeparam>
//    /// <typeparam name="V2">The value type associated with the target context.</typeparam>
//    /// <returns>A new instance of <see cref="AdapterPipeBlock{C1, V1, C2, V2}"/> configured with the provided parameters.</returns>
//    public static AdapterPipeBlock<C1, V1, C2, V2> CreatePipe<C1, V1, C2, V2>(this BlockBuilder<C1, V1> _,
//        string pipeName,
//        BlockBuilder<C2, V2> builder,
//        IAdapter<C1, V1, C2, V2> adapter)
//        where C1 : IContext<V1>
//        where C2 : IContext<V2>
//    => new(pipeName, adapter, builder);
//}