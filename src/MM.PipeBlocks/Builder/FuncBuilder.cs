using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;
/// <summary>
/// Provides methods to execute synchronous and asynchronous functions or actions within a block context.
/// </summary>
public partial class BlockBuilder<V>
{
    /// <summary>
    /// Executes a synchronous function that transforms the context.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<V> Run(Func<Parameter<V>, Parameter<V>> func) => new(func);

    /// <summary>
    /// Executes a synchronous action that operates on the context.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<V> Run(Action<Parameter<V>> action) => new(action);

    /// <summary>
    /// Executes a parameterless synchronous action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>FuncBlock</returns>
    public FuncBlock<V> Run(Action action) => new(v => action());

    /// <summary>
    /// Executes a parameterless asynchronous function.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<V> Run(Func<ValueTask> func) => new(v => func());

    /// <summary>
    /// Executes a parameterless asynchronous function.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<V> Run(Func<Task> func) => new(v => new ValueTask(func()));

    /// <summary>
    /// Executes an asynchronous function that transforms the context.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<V> Run(Func<Parameter<V>, ValueTask<Parameter<V>>> func) => new(func);

    /// <summary>
    /// Executes an asynchronous action that operates on the context.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <returns>AsyncFuncBlock</returns>
    public AsyncFuncBlock<V> Run(Func<Parameter<V>, ValueTask> action) => new(action);
}