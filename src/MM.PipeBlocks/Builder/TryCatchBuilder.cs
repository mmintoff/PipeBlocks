using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Blocks;

namespace MM.PipeBlocks;
/// <summary>
/// Provides methods for building exception-handling flow blocks such as try-catch, try-finally, and try-catch-finally.
/// </summary>
/// <typeparam name="C">The type of the context, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public partial class BlockBuilder<C, V>
    where C : IContext<V>
{
    #region TryCatch

    /// <summary>
    /// Wraps the given <paramref name="tryBlock"/> in a try-catch structure, using <paramref name="catchBlock"/> to handle exceptions.
    /// </summary>
    /// <param name="tryBlock">The block to execute in the try section.</param>
    /// <param name="catchBlock">The block to execute if an exception is thrown.</param>
    public TryCatchBlock<C, V> TryCatch(IBlock<C, V> tryBlock, IBlock<C, V> catchBlock)
        => new(CreateLogger<TryCatchBlock<C, V>>(), tryBlock, catchBlock, null);

    /// <summary>
    /// Wraps the specified generic blocks in a try-catch structure.
    /// </summary>
    /// <typeparam name="X">The block to execute in the try section.</typeparam>
    /// <typeparam name="Y">The block to execute in the catch section.</typeparam>
    public TryCatchBlock<C, V> TryCatch<X, Y>()
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        => new(CreateLogger<TryCatchBlock<C, V>>(), ResolveInstance<X>(), ResolveInstance<Y>(), null);

    #endregion

    #region TryFinally

    /// <summary>
    /// Wraps the given <paramref name="tryBlock"/> in a try-finally structure, ensuring <paramref name="finallyBlock"/> is always executed.
    /// </summary>
    /// <param name="tryBlock">The block to execute in the try section.</param>
    /// <param name="finallyBlock">The block to execute in the finally section.</param>
    public TryCatchBlock<C, V> TryFinally(IBlock<C, V> tryBlock, IBlock<C, V> finallyBlock)
        => new(CreateLogger<TryCatchBlock<C, V>>(), tryBlock, null, finallyBlock);

    /// <summary>
    /// Wraps the specified generic blocks in a try-finally structure.
    /// </summary>
    /// <typeparam name="X">The block to execute in the try section.</typeparam>
    /// <typeparam name="Y">The block to execute in the finally section.</typeparam>
    public TryCatchBlock<C, V> TryFinally<X, Y>()
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        => new(CreateLogger<TryCatchBlock<C, V>>(), ResolveInstance<X>(), null, ResolveInstance<Y>());

    #endregion

    #region TryCatchFinally

    /// <summary>
    /// Wraps the given blocks in a full try-catch-finally structure.
    /// </summary>
    /// <param name="tryBlock">The block to execute in the try section.</param>
    /// <param name="catchBlock">The block to execute in the catch section.</param>
    /// <param name="finallyBlock">The block to execute in the finally section.</param>
    public TryCatchBlock<C, V> TryCatchFinally(IBlock<C, V> tryBlock, IBlock<C, V> catchBlock, IBlock<C, V> finallyBlock)
        => new(CreateLogger<TryCatchBlock<C, V>>(), tryBlock, catchBlock, finallyBlock);

    /// <summary>
    /// Wraps the specified generic blocks in a full try-catch-finally structure.
    /// </summary>
    /// <typeparam name="X">The block to execute in the try section.</typeparam>
    /// <typeparam name="Y">The block to execute in the catch section.</typeparam>
    /// <typeparam name="Z">The block to execute in the finally section.</typeparam>
    public TryCatchBlock<C, V> TryCatchFinally<X, Y, Z>()
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        where Z : IBlock<C, V>
        => new(CreateLogger<TryCatchBlock<C, V>>(), ResolveInstance<X>(), ResolveInstance<Y>(), ResolveInstance<Z>());

    #endregion
}