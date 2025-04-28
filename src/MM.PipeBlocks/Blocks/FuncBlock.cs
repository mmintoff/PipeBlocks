using MM.PipeBlocks.Abstractions;
using System;
using System.Reflection;

namespace MM.PipeBlocks.Blocks;
/// <summary>
/// A synchronous block that executes a function or action with optional access to the context's value.
/// </summary>
/// <typeparam name="C">The context type implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public class FuncBlock<C, V> : ISyncBlock<C, V>
    where C : IContext<V>
{
    private readonly bool _isFunc;
    private readonly Either<Func<C, C>, Func<C, V, C>>? _func;
    private readonly Either<Action<C>, Action<C, V>>? _action;
    private readonly string _fullName;

    /// <summary>
    /// Initializes a new instance using a function that transforms the context.
    /// </summary>
    public FuncBlock(Func<C, C> func) => (_func, _isFunc, _fullName) = (func, true, GetFullName(func?.Method));

    /// <summary>
    /// Initializes a new instance using a function that transforms the context and takes a value.
    /// </summary>
    public FuncBlock(Func<C, V, C> func) => (_func, _isFunc, _fullName) = (func, true, GetFullName(func?.Method));

    /// <summary>
    /// Initializes a new instance using an action that operates on the context.
    /// </summary>
    public FuncBlock(Action<C> action) => (_action, _isFunc, _fullName) = (action, false, GetFullName(action?.Method));

    /// <summary>
    /// Initializes a new instance using an action that operates on the context and takes a value.
    /// </summary>
    public FuncBlock(Action<C, V> action) => (_action, _isFunc, _fullName) = (action, false, GetFullName(action?.Method));

    /// <summary>
    /// Executes the function or action against the given context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The updated context.</returns>
    public C Execute(C context)
    {
        return context.Value.Match(
            x => context.IsFlipped ? Execute(context, x.Value) : context,
            x => Execute(context, x));

        C Execute(C context, V value)
        {
            if (_isFunc)
            {
                return _func!.Match(
                    f => f(context),
                    f => f(context, value));
            }
            else
            {
                _action!.Match(
                    f => f(context),
                    f => f(context, value));
                return context;
            }
        }
    }

    string GetFullName(MethodInfo? method)
        => $"{base.ToString() ?? nameof(FuncBlock<C, V>)} (Method: {method?.DeclaringType?.FullName ?? "UnknownType"}.{method?.Name ?? "UnknownMethod"})";

    public override string ToString()
        => _fullName;
}

/// <summary>
/// An asynchronous block that executes a function or action with optional access to the context's value.
/// </summary>
/// <typeparam name="C">The context type implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public class AsyncFuncBlock<C, V> : IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly bool _isFunc;
    private readonly Either<Func<C, ValueTask<C>>, Func<C, V, ValueTask<C>>>? _func;
    private readonly Either<Func<C, ValueTask>, Func<C, V, ValueTask>>? _action;
    private readonly string _fullName;

    /// <summary>
    /// Initializes a new instance using an asynchronous function that returns a context.
    /// </summary>
    public AsyncFuncBlock(Func<C, ValueTask<C>> func) => (_func, _isFunc, _fullName) = (func, true, GetFullName(func?.Method));

    /// <summary>
    /// Initializes a new instance using an asynchronous function that returns a context and accepts a value.
    /// </summary>
    public AsyncFuncBlock(Func<C, V, ValueTask<C>> func) => (_func, _isFunc, _fullName) = (func, true, GetFullName(func?.Method));

    /// <summary>
    /// Initializes a new instance using an asynchronous action.
    /// </summary>
    public AsyncFuncBlock(Func<C, ValueTask> action) => (_action, _isFunc, _fullName) = (action, false, GetFullName(action?.Method));

    /// <summary>
    /// Initializes a new instance using an asynchronous action that accepts a value.
    /// </summary>
    public AsyncFuncBlock(Func<C, V, ValueTask> action) => (_action, _isFunc, _fullName) = (action, false, GetFullName(action?.Method));

    /// <summary>
    /// Executes the asynchronous function or action against the given context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated context.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        return await context.Value.MatchAsync(
            x => context.IsFlipped ? ExecuteAsync(context, x.Value) : ValueTask.FromResult(context),
            x => ExecuteAsync(context, x));

        async ValueTask<C> ExecuteAsync(C context, V value)
        {
            if (_isFunc)
            {
                return await _func!.MatchAsync(
                    f => f(context),
                    f => f(context, value));
            }
            else
            {
                await _action!.MatchAsync(
                    f => f(context),
                    f => f(context, value));
                return context;
            }
        }
    }

    string GetFullName(MethodInfo? method)
        => $"{base.ToString() ?? nameof(AsyncFuncBlock<C, V>)} (Method: {method?.DeclaringType?.FullName ?? "UnknownType"}.{method?.Name ?? "UnknownMethod"})";

    public override string ToString()
        => _fullName;
}