using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Blocks;

namespace MM.PipeBlocks;
/// <summary>
/// Provides methods to execute synchronous and asynchronous functions or actions within a block context.
/// </summary>
public partial class BlockBuilder<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Executes a synchronous function that transforms the context.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<C, V> Run(Func<C, C> func) => new(func);

    /// <summary>
    /// Executes a synchronous function that takes the context and a value, and returns a new context.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<C, V> Run(Func<C, V, C> func) => new(func);

    /// <summary>
    /// Executes a synchronous action that operates on the context.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<C, V> Run(Action<C> action) => new(action);

    /// <summary>
    /// Executes a synchronous action that takes both the context and a value.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<C, V> Run(Action<C, V> action) => new(action);

    /// <summary>
    /// Executes a parameterless synchronous action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<C, V> Run(Action action) => new(c => action());

    /// <summary>
    /// Executes a parameterless asynchronous function.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<C, V> Run(Func<ValueTask> func) => new(c => func());

    /// <summary>
    /// Executes a parameterless asynchronous function.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<C, V> Run(Func<Task> func) => new(c => new ValueTask(func()));

    /// <summary>
    /// Executes an asynchronous function that transforms the context.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<C, V> Run(Func<C, ValueTask<C>> func) => new(func);

    /// <summary>
    /// Executes an asynchronous function that takes the context and a value, and returns a new context.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<C, V> Run(Func<C, V, ValueTask<C>> func) => new(func);

    /// <summary>
    /// Executes an asynchronous action that operates on the context.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<C, V> Run(Func<C, ValueTask> action) => new(action);

    /// <summary>
    /// Executes an asynchronous action that takes both the context and a value.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<C, V> Run(Func<C, V, ValueTask> action) => new(action);
}