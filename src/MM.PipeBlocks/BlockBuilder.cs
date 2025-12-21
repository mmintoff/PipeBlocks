using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Internal;

namespace MM.PipeBlocks;
/// <summary>
/// Provides functionality to build and resolve blocks for a specific context and value type.
/// </summary>
/// <typeparam name="C">The context type that implements <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value in the context.</typeparam>
/// <param name="resolver">The block resolver used to resolve block instances.</param>
/// <param name="loggerFactory">The logger factory used to create loggers for blocks.</param>
public partial class BlockBuilder<V>(IBlockResolver<V> resolver, ILoggerFactory loggerFactory)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockBuilder{C, V}"/> class using the default block resolver and NoopLoggerFactory.
    /// </summary>
    public BlockBuilder()
        : this(new DefaultBlockResolver<V>(), new NoopLoggerFactory()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockBuilder{C, V}"/> class using the default block resolver.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public BlockBuilder(ILoggerFactory loggerFactory)
        : this(new DefaultBlockResolver<V>(), loggerFactory) { }

    /// <summary>
    /// Creates a new <see cref="PipeBlock{C, V}"/> with the specified name.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <returns>A new instance of <see cref="PipeBlock{C, V}"/>.</returns>
    public PipeBlock<V> CreatePipe(string pipeName)
        => new(pipeName, this);

    /// <summary>
    /// Resolves an instance of the specified block type using the configured resolver.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <returns>An instance of the specified block type.</returns>
    public X ResolveInstance<X>()
        where X : IBlock<V>
        => resolver.ResolveInstance<X>();

    /// <summary>
    /// Creates a logger for the specified type.
    /// </summary>
    /// <typeparam name="X">The type for which to create a logger.</typeparam>
    /// <returns>An <see cref="ILogger{X}"/> instance.</returns>
    public ILogger<X> CreateLogger<X>()
        => loggerFactory.CreateLogger<X>();
}