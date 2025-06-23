using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;
/// <summary>
/// Provides methods to build conditional and branching blocks in a control-flow graph.
/// </summary>
public partial class BlockBuilder<C, V>
    where C : IContext<V>
{
    #region Switch

    /// <summary>
    /// Creates a branching block based on a function that takes the context and returns the next block.
    /// </summary>
    public BranchBlock<C, V> Switch(Func<C, IBlock<C, V>> nextBlockFunc)
        => new(nextBlockFunc);

    /// <summary>
    /// Creates a branching block based on a function that takes the context and value, and returns the next block.
    /// </summary>
    public BranchBlock<C, V> Switch(Func<C, V, IBlock<C, V>> nextBlockFunc)
        => new(nextBlockFunc);

    /// <summary>
    /// Creates an asynchronous branching block based on a function that takes the context and returns a task of the next block.
    /// </summary>
    public BranchBlock<C, V> Switch(Func<C, ValueTask<IBlock<C, V>>> nextBlockFunc)
        => new(nextBlockFunc);

    /// <summary>
    /// Creates an asynchronous branching block based on a function that takes the context and value, and returns a task of the next block.
    /// </summary>
    public BranchBlock<C, V> Switch(Func<C, V, ValueTask<IBlock<C, V>>> nextBlockFunc)
        => new(nextBlockFunc);

    #endregion

    #region If-Then-Else Non-Generic

    /// <summary>
    /// Executes <paramref name="doThis"/> if the condition is true; otherwise, executes <paramref name="elseThis"/>.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, bool> condition, IBlock<C, V> doThis, IBlock<C, V> elseThis)
        => Switch(context => context.Value.Match(
                _ => context.IsFlipped && condition(context),
                _ => condition(context)) ? doThis : elseThis);

    /// <summary>
    /// Executes <paramref name="doThis"/> if the value-based condition is true; otherwise, executes <paramref name="elseThis"/>.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, V, bool> condition, IBlock<C, V> doThis, IBlock<C, V> elseThis)
        => Switch(context => context.Value.Match(
                x => context.IsFlipped && condition(context, x.Value),
                x => condition(context, x)) ? doThis : elseThis);

    /// <summary>
    /// Asynchronously evaluates a condition and branches accordingly.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, ValueTask<bool>> condition, IBlock<C, V> doThis, IBlock<C, V> elseThis)
        => Switch(async context => await context.Value.MatchAsync(
                _ => context.IsFlipped ? condition(context) : ValueTask.FromResult(false),
                _ => condition(context)) ? doThis : elseThis);

    /// <summary>
    /// Asynchronously evaluates a value-based condition and branches accordingly.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, V, ValueTask<bool>> condition, IBlock<C, V> doThis, IBlock<C, V> elseThis)
        => Switch(async context => await context.Value.MatchAsync(
                x => context.IsFlipped ? condition(context, x.Value) : ValueTask.FromResult(false),
                x => condition(context, x)) ? doThis : elseThis);

    #endregion

    #region If-Then-Else Generic

    /// <summary>
    /// Conditionally executes one of two generic blocks depending on the boolean result of a context-based condition.
    /// </summary>
    public BranchBlock<C, V> IfThen<X, Y>(Func<C, bool> condition)
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), ResolveInstance<Y>());

    /// <summary>
    /// Conditionally executes one of two generic blocks depending on a context and value-based condition.
    /// </summary>
    public BranchBlock<C, V> IfThen<X, Y>(Func<C, V, bool> condition)
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), ResolveInstance<Y>());

    /// <summary>
    /// Asynchronously evaluates a context-based condition and executes the appropriate generic block.
    /// </summary>
    public BranchBlock<C, V> IfThen<X, Y>(Func<C, ValueTask<bool>> condition)
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), ResolveInstance<Y>());

    /// <summary>
    /// Asynchronously evaluates a context and value-based condition and executes the appropriate generic block.
    /// </summary>
    public BranchBlock<C, V> IfThen<X, Y>(Func<C, V, ValueTask<bool>> condition)
        where X : IBlock<C, V>
        where Y : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), ResolveInstance<Y>());

    #endregion

    #region If-Then Non-Generic

    /// <summary>
    /// Executes <paramref name="doThis"/> if the context-based condition is true, otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, bool> condition, IBlock<C, V> doThis)
        => IfThen(condition, doThis, Noop());

    /// <summary>
    /// Executes <paramref name="doThis"/> if the context and value-based condition is true, otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, V, bool> condition, IBlock<C, V> doThis)
        => IfThen(condition, doThis, Noop());

    /// <summary>
    /// Asynchronously evaluates a context-based condition and executes <paramref name="doThis"/> if true, otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, ValueTask<bool>> condition, IBlock<C, V> doThis)
        => IfThen(condition, doThis, Noop());

    /// <summary>
    /// Asynchronously evaluates a context and value-based condition and executes <paramref name="doThis"/> if true, otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen(Func<C, V, ValueTask<bool>> condition, IBlock<C, V> doThis)
        => IfThen(condition, doThis, Noop());

    #endregion

    #region If-Then Generic

    /// <summary>
    /// Conditionally executes a generic block if the context-based condition is true; otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen<X>(Func<C, bool> condition)
        where X : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), Noop());

    /// <summary>
    /// Conditionally executes a generic block if the context and value-based condition is true; otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen<X>(Func<C, V, bool> condition)
        where X : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), Noop());

    /// <summary>
    /// Asynchronously evaluates a context-based condition and executes a generic block if true; otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen<X>(Func<C, ValueTask<bool>> condition)
        where X : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), Noop());

    /// <summary>
    /// Asynchronously evaluates a context and value-based condition and executes a generic block if true; otherwise breaks.
    /// </summary>
    public BranchBlock<C, V> IfThen<X>(Func<C, V, ValueTask<bool>> condition)
        where X : IBlock<C, V>
        => IfThen(condition, ResolveInstance<X>(), Noop());

    #endregion
}