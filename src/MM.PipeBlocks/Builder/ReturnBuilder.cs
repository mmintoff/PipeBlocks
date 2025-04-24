using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Blocks;

namespace MM.PipeBlocks;
public partial class BlockBuilder<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Create a Break block
    /// </summary>
    /// <returns>BreakBlock</returns>
    public BreakBlock<C, V> Break() => new();

    #region Synchronous
    /// <summary>
    /// Default synchronous Return method that returns the same context.
    /// </summary>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return() => new(CreateLogger<ReturnBlock<C, V>>(), c => c);

    /// <summary>
    /// Synchronous Return method that accepts an Action and executes it on the context.
    /// </summary>
    /// <param name="action">The action to perform on the context.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Action<C> action)
        => Return(context =>
        {
            action(context);
            return context;
        });

    /// <summary>
    /// Synchronous Return method that accepts a Func and executes it on the context.
    /// </summary>
    /// <param name="func">The function to apply to the context.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Func<C, C> func)
        => new(CreateLogger<ReturnBlock<C, V>>(), func);

    /// <summary>
    /// Conditional synchronous Return method that executes if the predicate is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition)
        => ReturnIf(condition, context => context);

    /// <summary>
    /// Conditional synchronous Return method that accepts an Action and executes it if the predicate is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="action">The action to perform on the context if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Action<C> action)
        => ReturnIf(condition, context =>
        {
            action(context);
            return context;
        });

    /// <summary>
    /// Conditional synchronous Return method that accepts a Func and executes it if the predicate is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="func">The function to apply to the context if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, bool> condition, Func<C, C> func)
        => IfThen(condition, Return(func), Break());

    /// <summary>
    /// Synchronous Return method that accepts an Action with two parameters (context and value) and executes it.
    /// </summary>
    /// <param name="action">The action to perform on the context and value.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Action<C, V> action)
        => Return((context, value) =>
        {
            action(context, value);
            return context;
        });

    /// <summary>
    /// Synchronous Return method that accepts a Func with two parameters (context and value) and executes it.
    /// </summary>
    /// <param name="func">The function to apply to the context and value.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Func<C, V, C> func)
        => new(CreateLogger<ReturnBlock<C, V>>(), func);

    /// <summary>
    /// Conditional synchronous Return method with two parameters that executes if the predicate is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, V, bool> condition)
        => ReturnIf(condition, (context, value) => context);

    /// <summary>
    /// Conditional synchronous Return method with two parameters that accepts an Action and executes it if the predicate is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="action">The action to perform on the context and value if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, V, bool> condition, Action<C, V> action)
        => ReturnIf(condition, (context, value) =>
        {
            action(context, value);
            return context;
        });

    /// <summary>
    /// Conditional synchronous Return method with two parameters that accepts a Func and executes it if the predicate is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="func">The function to apply to the context and value if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, V, bool> condition, Func<C, V, C> func)
        => IfThen(condition, Return(func), Break());
    #endregion

    #region Asynchronous
    /// <summary>
    /// Asynchronous Return method that accepts a Func returning a ValueTask and executes it on the context.
    /// </summary>
    /// <param name="action">The asynchronous action to perform on the context.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Func<C, ValueTask> action)
        => Return(async context =>
        {
            await action(context);
            return context;
        });

    /// <summary>
    /// Asynchronous Return method that accepts a Func returning a ValueTask&lt;C&gt; and executes it on the context.
    /// </summary>
    /// <param name="func">The asynchronous function to apply to the context.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Func<C, ValueTask<C>> func)
        => new(CreateLogger<ReturnBlock<C, V>>(), func);

    /// <summary>
    /// Conditional asynchronous Return method that executes if the predicate evaluates to true.
    /// </summary>
    /// <param name="condition">The asynchronous condition to evaluate.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition)
        => ReturnIf(condition, context => ValueTask.FromResult(context));

    /// <summary>
    /// Conditional asynchronous Return method that accepts a Func returning a ValueTask and executes it if the predicate evaluates to true.
    /// </summary>
    /// <param name="condition">The asynchronous condition to evaluate.</param>
    /// <param name="action">The asynchronous action to perform on the context if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, ValueTask> action)
        => ReturnIf(condition, async context =>
        {
            await action(context);
            return context;
        });

    /// <summary>
    /// Conditional asynchronous Return method that accepts a Func returning a ValueTask&lt;C&gt; and executes it if the predicate evaluates to true.
    /// </summary>
    /// <param name="condition">The asynchronous condition to evaluate.</param>
    /// <param name="func">The asynchronous function to apply to the context if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, ValueTask<bool>> condition, Func<C, ValueTask<C>> func)
        => IfThen(condition, Return(func), Break());

    /// <summary>
    /// Asynchronous Return method that accepts a Func with two parameters (context and value) returning a ValueTask and executes it.
    /// </summary>
    /// <param name="action">The asynchronous action to perform on the context and value.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Func<C, V, ValueTask> action)
        => Return(async (context, value) =>
        {
            await action(context, value);
            return context;
        });

    /// <summary>
    /// Asynchronous Return method that accepts a Func with two parameters (context and value) returning a ValueTask&lt;C&gt; and executes it.
    /// </summary>
    /// <param name="func">The asynchronous function to apply to the context and value.</param>
    /// <returns>ReturnBlock</returns>
    public ReturnBlock<C, V> Return(Func<C, V, ValueTask<C>> func)
        => new(CreateLogger<ReturnBlock<C,V>>(), func);

    /// <summary>
    /// Conditional asynchronous Return method with two parameters that executes if the predicate evaluates to true.
    /// </summary>
    /// <param name="condition">The asynchronous condition to evaluate.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, V, ValueTask<bool>> condition)
        => ReturnIf(condition, context => ValueTask.FromResult(context));

    /// <summary>
    /// Conditional asynchronous Return method with two parameters that accepts a Func returning a ValueTask and executes it if the predicate evaluates to true.
    /// </summary>
    /// <param name="condition">The asynchronous condition to evaluate.</param>
    /// <param name="action">The asynchronous action to perform on the context and value if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, V, ValueTask<bool>> condition, Func<C, ValueTask> action)
        => ReturnIf(condition, async context =>
        {
            await action(context);
            return context;
        });

    /// <summary>
    /// Conditional asynchronous Return method with two parameters that accepts a Func returning a ValueTask&lt;C&gt; and executes it if the predicate evaluates to true.
    /// </summary>
    /// <param name="condition">The asynchronous condition to evaluate.</param>
    /// <param name="func">The asynchronous function to apply to the context and value if the predicate is true.</param>
    /// <returns>BranchBlock</returns>
    public BranchBlock<C, V> ReturnIf(Func<C, V, ValueTask<bool>> condition, Func<C, ValueTask<C>> func)
        => IfThen(condition, Return(func), Break());
    #endregion
}