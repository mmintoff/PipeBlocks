using MM.PipeBlocks.Abstractions;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// A synchronous block that executes a function or action with optional access to the context's value.
/// </summary>
/// <typeparam name="C">The context type implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public class FuncBlock<C, V> : ISyncBlock<C, V>
    where C : IContext<V>
{
    private readonly Func<C, C>? _contextFunc;
    private readonly Func<C, V, C>? _contextValueFunc;
    private readonly Action<C>? _contextAction;
    private readonly Action<C, V>? _contextValueAction;

    private readonly ExecutionStrategy _executionStrategy;

    private string? _fullName;

    private enum ExecutionStrategy : byte
    {
        ContextFunc,
        ContextValueFunc,
        ContextAction,
        ContextValueAction
    }

    /// <summary>
    /// Initializes a new instance using a function that transforms the context.
    /// </summary>
    public FuncBlock(Func<C, C> func)
    {
        _contextFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.ContextFunc;
    }

    /// <summary>
    /// Initializes a new instance using a function that transforms the context and takes a value.
    /// </summary>
    public FuncBlock(Func<C, V, C> func)
    {
        _contextValueFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.ContextValueFunc;
    }

    /// <summary>
    /// Initializes a new instance using an action that operates on the context.
    /// </summary>
    public FuncBlock(Action<C> action)
    {
        _contextAction = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.ContextAction;
    }

    /// <summary>
    /// Initializes a new instance using an action that operates on the context and takes a value.
    /// </summary>
    public FuncBlock(Action<C, V> action)
    {
        _contextValueAction = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.ContextValueAction;
    }

    /// <summary>
    /// Executes the function or action against the given context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The updated context.</returns>
    public C Execute(C context)
    {
        return context.Value.Match(
            f => context.IsFlipped ? ExecuteWithValue(context, f.Value) : context,
            s => ExecuteWithValue(context, s));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private C ExecuteWithValue(C context, V value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.ContextFunc => _contextFunc!(context),
            ExecutionStrategy.ContextValueFunc => _contextValueFunc!(context, value),
            ExecutionStrategy.ContextAction => ExecuteAction(_contextAction!, context),
            ExecutionStrategy.ContextValueAction => ExecuteAction(_contextValueAction!, context, value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static C ExecuteAction(Action<C> action, C context)
    {
        action(context);
        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static C ExecuteAction(Action<C, V> action, C context, V value)
    {
        action(context, value);
        return context;
    }

    public override string ToString() => _fullName ??= GetFullName();

    private string GetFullName()
    {
        var method = _executionStrategy switch
        {
            ExecutionStrategy.ContextFunc => _contextFunc?.Method,
            ExecutionStrategy.ContextValueFunc => _contextValueFunc?.Method,
            ExecutionStrategy.ContextAction => _contextAction?.Method,
            ExecutionStrategy.ContextValueAction => _contextValueAction?.Method,
            _ => null
        };

        var typeName = method?.DeclaringType?.FullName ?? "UnknownType";
        var methodName = method?.Name ?? "UnknownMethod";
        var baseName = base.ToString() ?? nameof(FuncBlock<C, V>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}

/// <summary>
/// An asynchronous block that executes a function or action with optional access to the context's value.
/// </summary>
/// <typeparam name="C">The context type implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public class AsyncFuncBlock<C, V> : IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly Func<C, ValueTask<C>>? _contextFunc;
    private readonly Func<C, V, ValueTask<C>>? _contextValueFunc;
    private readonly Func<C, ValueTask>? _contextAction;
    private readonly Func<C, V, ValueTask>? _contextValueAction;

    private readonly ExecutionStrategy _executionStrategy;

    private string? _fullName;

    private enum ExecutionStrategy : byte
    {
        ContextFunc,
        ContextValueFunc,
        ContextAction,
        ContextValueAction
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that returns a context.
    /// </summary>
    public AsyncFuncBlock(Func<C, ValueTask<C>> func)
    {
        _contextFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.ContextFunc;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that returns a context and accepts a value.
    /// </summary>
    public AsyncFuncBlock(Func<C, V, ValueTask<C>> func)
    {
        _contextValueFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.ContextValueFunc;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous action.
    /// </summary>
    public AsyncFuncBlock(Func<C, ValueTask> action)
    {
        _contextAction = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.ContextAction;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous action that accepts a value.
    /// </summary>
    public AsyncFuncBlock(Func<C, V, ValueTask> action)
    {
        _contextValueAction = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.ContextValueAction;
    }

    /// <summary>
    /// Executes the asynchronous function or action against the given context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated context.</returns>
    public ValueTask<C> ExecuteAsync(C context)
    {
        return context.Value.Match(
            f => context.IsFlipped ? ExecuteWithValueAsync(context, f.Value) : new ValueTask<C>(context),
            s => ExecuteWithValueAsync(context, s));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<C> ExecuteWithValueAsync(C context, V value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.ContextFunc => _contextFunc!(context),
            ExecutionStrategy.ContextValueFunc => _contextValueFunc!(context, value),
            ExecutionStrategy.ContextAction => ExecuteActionAsync(_contextAction!, context),
            ExecutionStrategy.ContextValueAction => ExecuteActionAsync(_contextValueAction!, context, value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask<C> ExecuteActionAsync(Func<C, ValueTask> action, C context)
    {
        await action(context).ConfigureAwait(false);
        return context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask<C> ExecuteActionAsync(Func<C, V, ValueTask> action, C context, V value)
    {
        await action(context, value).ConfigureAwait(false);
        return context;
    }

    public override string ToString() => _fullName ??= GetFullName();

    private string GetFullName()
    {
        var method = _executionStrategy switch
        {
            ExecutionStrategy.ContextFunc => _contextFunc?.Method,
            ExecutionStrategy.ContextValueFunc => _contextValueFunc?.Method,
            ExecutionStrategy.ContextAction => _contextAction?.Method,
            ExecutionStrategy.ContextValueAction => _contextValueAction?.Method,
            _ => null
        };

        var typeName = method?.DeclaringType?.FullName ?? "UnknownType";
        var methodName = method?.Name ?? "UnknownMethod";
        var baseName = base.ToString() ?? nameof(AsyncFuncBlock<C, V>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}