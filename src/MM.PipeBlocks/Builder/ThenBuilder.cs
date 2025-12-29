using MM.PipeBlocks.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// Provides methods to build conditional and branching blocks in a control-flow graph.
/// </summary>
public partial class BlockBuilder<V>
{
    #region Switch

    /// <summary>
    /// Creates a branching block based on a function that takes the context and returns the next block.
    /// </summary>
    public BranchBlock<V> Switch(Func<Parameter<V>, IBlock<V>> nextBlockFunc)
        => new(nextBlockFunc);

    /// <summary>
    /// Creates an asynchronous branching block based on a function that takes the context and returns a task of the next block.
    /// </summary>
    public BranchBlock<V> Switch(Func<Parameter<V>, ValueTask<IBlock<V>>> nextBlockFunc)
        => new(nextBlockFunc);

    #endregion

    #region If-Then-Else Non-Generic

    /// <summary>
    /// Executes <paramref name="doThis"/> if the condition is true; otherwise, executes <paramref name="elseThis"/>.
    /// </summary>
    public BranchBlock<V> IfThen(Func<Parameter<V>, bool> condition, IBlock<V> doThis, IBlock<V> elseThis)
        => Switch(value => value.Match(
                _ => value.Context.IsFlipped && condition(value),
                _ => condition(value)) ? doThis : elseThis);

    /// <summary>
    /// Asynchronously evaluates a condition and branches accordingly.
    /// </summary>
    public BranchBlock<V> IfThen(Func<Parameter<V>, ValueTask<bool>> condition, IBlock<V> doThis, IBlock<V> elseThis)
        => Switch(async value => await value.MatchAsync(
                _ => value.Context.IsFlipped ? condition(value) : ValueTask.FromResult(false),
                _ => condition(value)) ? doThis : elseThis);

    #endregion

    #region If-Then-Else Generic

    /// <summary>
    /// Conditionally executes one of two generic blocks depending on the boolean result of a context-based condition.
    /// </summary>
    public BranchBlock<V> IfThen<X, Y>(Func<Parameter<V>, bool> condition)
        where X : IBlock<V>
        where Y : IBlock<V>
        => IfThen(condition, ResolveInstance<X>(), ResolveInstance<Y>());

    /// <summary>
    /// Asynchronously evaluates a context-based condition and executes the appropriate generic block.
    /// </summary>
    public BranchBlock<V> IfThen<X, Y>(Func<Parameter<V>, ValueTask<bool>> condition)
        where X : IBlock<V>
        where Y : IBlock<V>
        => IfThen(condition, ResolveInstance<X>(), ResolveInstance<Y>());

    #endregion

    #region If-Then Non-Generic

    /// <summary>
    /// Executes <paramref name="doThis"/> if the context-based condition is true, otherwise breaks.
    /// </summary>
    public BranchBlock<V> IfThen(Func<Parameter<V>, bool> condition, IBlock<V> doThis)
        => IfThen(condition, doThis, Noop());

    /// <summary>
    /// Asynchronously evaluates a context-based condition and executes <paramref name="doThis"/> if true, otherwise breaks.
    /// </summary>
    public BranchBlock<V> IfThen(Func<Parameter<V>, ValueTask<bool>> condition, IBlock<V> doThis)
        => IfThen(condition, doThis, Noop());

    #endregion

    #region If-Then Generic

    /// <summary>
    /// Conditionally executes a generic block if the context-based condition is true; otherwise breaks.
    /// </summary>
    public BranchBlock<V> IfThen<X>(Func<Parameter<V>, bool> condition)
        where X : IBlock<V>
        => IfThen(condition, ResolveInstance<X>(), Noop());

    /// <summary>
    /// Asynchronously evaluates a context-based condition and executes a generic block if true; otherwise breaks.
    /// </summary>
    public BranchBlock<V> IfThen<X>(Func<Parameter<V>, ValueTask<bool>> condition)
        where X : IBlock<V>
        => IfThen(condition, ResolveInstance<X>(), Noop());
    #endregion
}