using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;
/// <summary>
/// A fluent builder for composing pipeline blocks that can control flow,
/// return early, or do nothing (no-op).
/// </summary>
public partial class BlockBuilder<V>
{
    /// <summary>
    /// Creates a no-operation block that simply returns the input context unchanged.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="NoopBlock{C, V}"/> that performs no action.
    /// </returns>
    public NoopBlock<V> Noop() => new();

    /// <summary>
    /// Creates a return block that returns the input context unchanged.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that returns the same context without modification.
    /// </returns>
    public ReturnBlock<V> Return() => new(CreateLogger<ReturnBlock<V>>(), v => v);

    /// <summary>
    /// Creates a return block that executes the given action on the context and then returns it.
    /// </summary>
    /// <param name="doThis">An action to be performed on the context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that runs the specified action and returns the modified context.
    /// </returns>
    public ReturnBlock<V> Return(Action<Parameter<V>> doThis)
        => Return(context =>
        {
            doThis(context);
            return context;
        });

    /// <summary>
    /// Creates a return block that executes a transformation function on the context and returns the result.
    /// </summary>
    /// <param name="doThis">A function that transforms the context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that returns the result of the transformation.
    /// </returns>
    public ReturnBlock<V> Return(Func<Parameter<V>, Parameter<V>> doThis)
        => new(CreateLogger<ReturnBlock<V>>(), doThis);

    /// <summary>
    /// Creates a return block that performs an asynchronous action on the context and then returns it.
    /// </summary>
    /// <param name="doThis">An asynchronous function to be executed on the context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that awaits the specified action and then returns the context.
    /// </returns>
    public ReturnBlock<V> Return(Func<Parameter<V>, ValueTask> doThis)
        => Return(async context =>
        {
            await doThis(context);
            return context;
        });

    /// <summary>
    /// Creates a return block that performs an asynchronous transformation on the context and returns the result.
    /// </summary>
    /// <param name="doThis">An asynchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that executes the asynchronous transformation.
    /// </returns>
    public ReturnBlock<V> Return(Func<Parameter<V>, ValueTask<Parameter<V>>> doThis)
        => new(CreateLogger<ReturnBlock<V>>(), doThis);

    /// <summary>
    /// Creates a conditional return block that returns the context unchanged if the given condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies a no-op if the condition is true.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, bool> condition)
        => ReturnIf(condition, value => value);

    /// <summary>
    /// Creates a conditional return block that executes the given action and returns the context if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">An action to be executed on the context if the condition is true.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the action and returns the context if the condition is true,
    /// or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, bool> condition, Action<Parameter<V>> doThis)
        => IfThen(condition, Return(doThis), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given synchronous transformation using the context
    /// and returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">A synchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, bool> condition, Func<Parameter<V>, Parameter<V>> doThis)
        => IfThen(condition, Return(doThis), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous action using the context
    /// and returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">An asynchronous function that takes the context and performs an action, but returns the context unchanged.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, bool> condition, Func<Parameter<V>, ValueTask> doThis)
        => ReturnIf(condition, async value =>
        {
            await doThis(value);
            return value;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous transformation using the context
    /// and returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">An asynchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, bool> condition, Func<Parameter<V>, ValueTask<Parameter<V>>> doThis)
        => IfThen(condition, Return(doThis), Noop());


    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided synchronous action using the context, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">A synchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, ValueTask<bool>> condition, Action<Parameter<V>> doThis)
        => ReturnIf(condition, value =>
        {
            doThis(value);
            return value;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided synchronous transformation using the context and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">A synchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, ValueTask<bool>> condition, Func<Parameter<V>, Parameter<V>> doThis)
        => IfThen(condition, Return(doThis), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided asynchronous action using the context, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">An asynchronous function that takes the context and performs an action, but returns the context unchanged.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, ValueTask<bool>> condition, Func<Parameter<V>, ValueTask> doThis)
        => ReturnIf(condition, async value =>
        {
            await doThis(value);
            return value;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided asynchronous transformation using the context and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="doThis">An asynchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<V> ReturnIf(Func<Parameter<V>, ValueTask<bool>> condition, Func<Parameter<V>, ValueTask<Parameter<V>>> doThis)
        => IfThen(condition, Return(doThis), Noop());
}