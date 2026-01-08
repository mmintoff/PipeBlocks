using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;
/// <summary>
/// Provides methods for building exception-handling flow blocks such as try-catch, try-finally, and try-catch-finally.
/// </summary>
public partial class BlockBuilder<V>
{
    #region TryCatch

    /// <summary>
    /// Wraps the given <paramref name="tryThis"/> in a try-catch structure, using <paramref name="elseThis"/> to handle exceptions.
    /// </summary>
    /// <param name="tryThis">The block to execute in the try section.</param>
    /// <param name="elseThis">The block to execute if an exception is thrown.</param>
    public TryCatchBlock<V> TryCatch(IBlock<V> tryThis, IBlock<V> elseThis, bool shouldThrow = false)
        => new(CreateLogger<TryCatchBlock<V>>(), tryThis, elseThis, null, shouldThrow);

    /// <summary>
    /// Wraps the specified generic blocks in a try-catch structure.
    /// </summary>
    /// <typeparam name="X">The block to execute in the try section.</typeparam>
    /// <typeparam name="Y">The block to execute in the catch section.</typeparam>
    public TryCatchBlock<V> TryCatch<X, Y>(bool shouldThrow = false)
        where X : IBlock<V>
        where Y : IBlock<V>
        => new(CreateLogger<TryCatchBlock<V>>(), ResolveInstance<X>(), ResolveInstance<Y>(), null, shouldThrow);

    #endregion

    #region TryFinally

    /// <summary>
    /// Wraps the given <paramref name="tryThis"/> in a try-finally structure, ensuring <paramref name="finallyThis"/> is always executed.
    /// </summary>
    /// <param name="tryThis">The block to execute in the try section.</param>
    /// <param name="finallyThis">The block to execute in the finally section.</param>
    public TryCatchBlock<V> TryFinally(IBlock<V> tryThis, IBlock<V> finallyThis, bool shouldThrow = false)
        => new(CreateLogger<TryCatchBlock<V>>(), tryThis, null, finallyThis, shouldThrow);

    /// <summary>
    /// Wraps the specified generic blocks in a try-finally structure.
    /// </summary>
    /// <typeparam name="X">The block to execute in the try section.</typeparam>
    /// <typeparam name="Y">The block to execute in the finally section.</typeparam>
    public TryCatchBlock<V> TryFinally<X, Y>(bool shouldThrow = false)
        where X : IBlock<V>
        where Y : IBlock<V>
        => new(CreateLogger<TryCatchBlock<V>>(), ResolveInstance<X>(), null, ResolveInstance<Y>(), shouldThrow);

    #endregion

    #region TryCatchFinally

    /// <summary>
    /// Wraps the given blocks in a full try-catch-finally structure.
    /// </summary>
    /// <param name="tryThis">The block to execute in the try section.</param>
    /// <param name="elseThis">The block to execute in the catch section.</param>
    /// <param name="finallyThis">The block to execute in the finally section.</param>
    public TryCatchBlock<V> TryCatchFinally(IBlock<V> tryThis, IBlock<V> elseThis, IBlock<V> finallyThis, bool shouldThrow = false)
        => new(CreateLogger<TryCatchBlock<V>>(), tryThis, elseThis, finallyThis, shouldThrow);

    /// <summary>
    /// Wraps the specified generic blocks in a full try-catch-finally structure.
    /// </summary>
    /// <typeparam name="X">The block to execute in the try section.</typeparam>
    /// <typeparam name="Y">The block to execute in the catch section.</typeparam>
    /// <typeparam name="Z">The block to execute in the finally section.</typeparam>
    public TryCatchBlock<V> TryCatchFinally<X, Y, Z>(bool shouldThrow = false)
        where X : IBlock<V>
        where Y : IBlock<V>
        where Z : IBlock<V>
        => new(CreateLogger<TryCatchBlock<V>>(), ResolveInstance<X>(), ResolveInstance<Y>(), ResolveInstance<Z>(), shouldThrow);

    #endregion
}