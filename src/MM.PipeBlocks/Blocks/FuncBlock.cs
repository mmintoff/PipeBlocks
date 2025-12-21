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
public class FuncBlock<V> : ISyncBlock<V>
{
    private readonly Func<Parameter<V>, Parameter<V>>? _func;
    private readonly Action<Parameter<V>>? _action;

    private readonly ExecutionStrategy _executionStrategy;

    private string? _fullName;

    private enum ExecutionStrategy : byte
    {
        Func,
        Action
    }

    /// <summary>
    /// Initializes a new instance using a function that transforms the context.
    /// </summary>
    public FuncBlock(Func<Parameter<V>, Parameter<V>> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.Func;
    }

    /// <summary>
    /// Initializes a new instance using an action that operates on the context.
    /// </summary>
    public FuncBlock(Action<Parameter<V>> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.Action;
    }

    /// <summary>
    /// Executes the function or action against the given context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The updated context.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        return value.Match(
            f => Context.IsFlipped ? ExecuteWithValue(f.Value) : value,
            s => ExecuteWithValue(s));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Parameter<V> ExecuteWithValue(Parameter<V> value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.Func => _func!(value),
            ExecutionStrategy.Action => ExecuteAction(_action!, value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<V> ExecuteAction(Action<Parameter<V>> action, Parameter<V> value)
    {
        action(value);
        return value;
    }

    public override string ToString() => _fullName ??= GetFullName();

    private string GetFullName()
    {
        var method = _executionStrategy switch
        {
            ExecutionStrategy.Func => _func?.Method,
            ExecutionStrategy.Action => _action?.Method,
            _ => null
        };

        var typeName = method?.DeclaringType?.FullName ?? "UnknownType";
        var methodName = method?.Name ?? "UnknownMethod";
        var baseName = base.ToString() ?? nameof(FuncBlock<V>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}

/// <summary>
/// An asynchronous block that executes a function or action with optional access to the context's value.
/// </summary>
/// <typeparam name="C">The context type implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public class AsyncFuncBlock<V> : IAsyncBlock<V>
{
    private readonly Func<Parameter<V>, ValueTask<Parameter<V>>>? _func;
    private readonly Func<Parameter<V>, ValueTask>? _action;

    private readonly ExecutionStrategy _executionStrategy;

    private string? _fullName;

    private enum ExecutionStrategy : byte
    {
        Func,
        Action
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that returns a context.
    /// </summary>
    public AsyncFuncBlock(Func<Parameter<V>, ValueTask<Parameter<V>>> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.Func;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous action.
    /// </summary>
    public AsyncFuncBlock(Func<Parameter<V>, ValueTask> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.Action;
    }

    /// <summary>
    /// Executes the asynchronous function or action against the given context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated context.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        return value.Match(
            f => Context.IsFlipped ? ExecuteWithValueAsync(f.Value) : new ValueTask<Parameter<V>>(value),
            s => ExecuteWithValueAsync(s));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<Parameter<V>> ExecuteWithValueAsync(Parameter<V> value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.Func => _func!(value),
            ExecutionStrategy.Action => ExecuteActionAsync(_action!, value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask<Parameter<V>> ExecuteActionAsync(Func<Parameter<V>, ValueTask> action, Parameter<V> value)
    {
        await action(value).ConfigureAwait(false);
        return value;
    }

    public override string ToString() => _fullName ??= GetFullName();

    private string GetFullName()
    {
        var method = _executionStrategy switch
        {
            ExecutionStrategy.Func => _func?.Method,
            ExecutionStrategy.Action => _action?.Method,
            _ => null
        };

        var typeName = method?.DeclaringType?.FullName ?? "UnknownType";
        var methodName = method?.Name ?? "UnknownMethod";
        var baseName = base.ToString() ?? nameof(AsyncFuncBlock<V>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}