using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Blocks;

namespace MM.PipeBlocks;
/// <summary>
/// A fluent builder for composing pipeline blocks that can control flow,
/// return early, or do nothing (no-op).
/// </summary>
/// <typeparam name="C">The context type, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public partial class BlockBuilder<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Creates a no-operation block that simply returns the input context unchanged.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="NoopBlock{C, V}"/> that performs no action.
    /// </returns>
    public NoopBlock<C, V> Noop() => new();

    /// <summary>
    /// Creates a return block that returns the input context unchanged.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that returns the same context without modification.
    /// </returns>
    public ReturnBlock<C, V> Return() => new(CreateLogger<ReturnBlock<C, V>>(), c => c);

    /// <summary>
    /// Creates a return block that executes the given action on the context and then returns it.
    /// </summary>
    /// <param name="todo">An action to be performed on the context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that runs the specified action and returns the modified context.
    /// </returns>
    public ReturnBlock<C, V> Return(Action<C> todo)
        => Return(context =>
        {
            todo(context);
            return context;
        });

    /// <summary>
    /// Creates a return block that executes a transformation function on the context and returns the result.
    /// </summary>
    /// <param name="todo">A function that transforms the context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that returns the result of the transformation.
    /// </returns>
    public ReturnBlock<C, V> Return(Func<C, C> todo)
        => new(CreateLogger<ReturnBlock<C, V>>(), todo);

    /// <summary>
    /// Creates a return block that executes a transformation function using both the context and its value,
    /// and returns the resulting context.
    /// </summary>
    /// <param name="todo">A function that takes the context and its value to produce a new context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that returns the result of the transformation.
    /// </returns>
    public ReturnBlock<C, V> Return(Func<C, V, C> todo)
        => new(CreateLogger<ReturnBlock<C, V>>(), todo);

    /// <summary>
    /// Creates a return block that performs an asynchronous action on the context and then returns it.
    /// </summary>
    /// <param name="todo">An asynchronous function to be executed on the context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that awaits the specified action and then returns the context.
    /// </returns>
    public ReturnBlock<C, V> Return(Func<C, ValueTask> todo)
        => Return(async context =>
        {
            await todo(context);
            return context;
        });

    /// <summary>
    /// Creates a return block that performs an asynchronous transformation on the context and returns the result.
    /// </summary>
    /// <param name="todo">An asynchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that executes the asynchronous transformation.
    /// </returns>
    public ReturnBlock<C, V> Return(Func<C, ValueTask<C>> todo)
        => new(CreateLogger<ReturnBlock<C, V>>(), todo);

    /// <summary>
    /// Creates a return block that performs an asynchronous transformation using the context and its value,
    /// and returns the result.
    /// </summary>
    /// <param name="todo">An asynchronous function that takes the context and its value and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="ReturnBlock{C, V}"/> that executes the asynchronous transformation.
    /// </returns>
    public ReturnBlock<C, V> Return(Func<C, V, ValueTask<C>> todo)
        => new(CreateLogger<ReturnBlock<C, V>>(), todo);

    /// <summary>
    /// Creates a conditional return block that returns the context unchanged if the given condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies a no-op if the condition is true.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition)
        => ReturnIf(condition, context => context);

    /// <summary>
    /// Creates a conditional return block that executes the given action and returns the context if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An action to be executed on the context if the condition is true.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the action and returns the context if the condition is true,
    /// or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Action<C> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given action using both the context and its value,
    /// then returns the context if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An action to be executed on the context and its value if the condition is true.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the action and returns the context if the condition is true,
    /// or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Action<C, V> todo)
        => ReturnIf(condition, (context, value) =>
        {
            todo(context, value);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given synchronous transformation using the context
    /// and returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">A synchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, C> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given synchronous transformation using both the context
    /// and its value, then returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">A synchronous function that takes the context and its value and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, V, C> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous action using the context
    /// and returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and performs an action, but returns the context unchanged.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, ValueTask> todo)
        => ReturnIf(condition, async context =>
        {
            await todo(context);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous action using both the context
    /// and its value, then returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and its value and performs an action, but returns the context unchanged.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, V, ValueTask> todo)
        => ReturnIf(condition, async (context, value) =>
        {
            await todo(context, value);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous transformation using the context
    /// and returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, ValueTask<C>> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous transformation using both the context
    /// and its value, then returns the result if the condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and its value and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, V, ValueTask<C>> todo)
        => IfThen(condition, Return(todo), Noop());


    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided synchronous action using the context, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">A synchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Action<C> todo)
        => ReturnIf(condition, context =>
        {
            todo(context);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided synchronous action using both the context and its value, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">A synchronous function that takes the context and its value and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Action<C, V> todo)
        => ReturnIf(condition, (context, value) =>
        {
            todo(context, value);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided synchronous transformation using the context and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">A synchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, C> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided synchronous transformation using both the context and its value, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">A synchronous function that takes the context and its value and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the synchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, V, C> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided asynchronous action using the context, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and performs an action, but returns the context unchanged.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, ValueTask> todo)
        => ReturnIf(condition, async context =>
        {
            await todo(context);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided asynchronous action using both the context and its value, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and its value and performs an action, but returns the context unchanged.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous action and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, V, ValueTask> todo)
        => ReturnIf(condition, async (context, value) =>
        {
            await todo(context, value);
            return context;
        });

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided asynchronous transformation using the context and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, ValueTask<C>> todo)
        => IfThen(condition, Return(todo), Noop());

    /// <summary>
    /// Creates a conditional return block that executes the given asynchronous condition, and if the condition is met,
    /// it executes the provided asynchronous transformation using both the context and its value, and returns the result.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the action should be executed.</param>
    /// <param name="todo">An asynchronous function that takes the context and its value and returns a modified context.</param>
    /// <returns>
    /// A new instance of <see cref="BranchBlock{C, V}"/> that applies the asynchronous transformation and returns the context
    /// if the condition is true, or performs no operation otherwise.
    /// </returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, V, ValueTask<C>> todo)
        => IfThen(condition, Return(todo), Noop());

}