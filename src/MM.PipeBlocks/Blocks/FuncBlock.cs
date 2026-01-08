using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Internal;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// A synchronous block that executes a function or action with optional access to the parameter's value.
/// </summary>
/// <typeparam name="V">The type of the value associated with the parameter.</typeparam>
public class FuncBlock<V> : ISyncBlock<V>
{
    private readonly Func<Parameter<V>, Parameter<V>>? _func;
    private readonly Action<Parameter<V>>? _action;

    private readonly ExecutionStrategy _executionStrategy;

    private string? _fullName;

    /// <summary>
    /// Initializes a new instance using a function that transforms the parameter.
    /// </summary>
    public FuncBlock(Func<Parameter<V>, Parameter<V>> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.Func;
    }

    /// <summary>
    /// Initializes a new instance using an action that operates on the parameter.
    /// </summary>
    public FuncBlock(Action<Parameter<V>> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _executionStrategy = ExecutionStrategy.Action;
    }

    /// <summary>
    /// Executes the function or action against the given parameter.
    /// </summary>
    /// <param name="value">The execution parameter.</param>
    /// <returns>The updated parameter.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return value;

        return ExecuteWithStrategy(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Parameter<V> ExecuteWithStrategy(Parameter<V> value)
        => _executionStrategy switch
        {
            ExecutionStrategy.Func => _func!(value),
            ExecutionStrategy.Action => ExecuteAction(_action!, value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };

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
        var baseName = base.ToString() ?? nameof(FuncBlock<>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}

public class FuncBlock<VIn, VOut> : ISyncBlock<VIn, VOut>
{
    private readonly Func<Parameter<VIn>, Parameter<VOut>> _func;

    private string? _fullName;

    public FuncBlock(Func<Parameter<VIn>, Parameter<VOut>> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    public Parameter<VOut> Execute(Parameter<VIn> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return new Parameter<VOut>(value.Failure)
            {
                Context = value.Context
            };

        return _func(value);
    }

    public override string ToString() => _fullName ??= GetFullName();

    private string GetFullName()
    {
        var method = _func?.Method;
        var typeName = method?.DeclaringType?.FullName ?? "UnknownType";
        var methodName = method?.Name ?? "UnknownMethod";
        var baseName = base.ToString() ?? nameof(FuncBlock<>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}

/// <summary>
/// An asynchronous block that executes a function or action with optional access to the parameter's value.
/// </summary>
/// <typeparam name="V">The type of the value associated with the parameter.</typeparam>
public class AsyncFuncBlock<V> : IAsyncBlock<V>
{
    private readonly Func<Parameter<V>, ValueTask<Parameter<V>>>? _func;
    private readonly Func<Parameter<V>, ValueTask>? _action;

    private readonly ExecutionStrategy _executionStrategy;

    private string? _fullName;

    /// <summary>
    /// Initializes a new instance using an asynchronous function that returns a parameter.
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
    /// Executes the asynchronous function or action against the given parameter.
    /// </summary>
    /// <param name="value">The execution parameter.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated parameter.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return ValueTask.FromResult(value);

        return ExecuteWithStrategyAsync(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<Parameter<V>> ExecuteWithStrategyAsync(Parameter<V> value)
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
        var baseName = base.ToString() ?? nameof(AsyncFuncBlock<>);

        return $"{baseName} (Method: {typeName}.{methodName})";
    }
}

public class AsyncFuncBlock<VIn, VOut> : IAsyncBlock<VIn, VOut>
{
    private readonly Func<Parameter<VIn>, ValueTask<Parameter<VOut>>> _func;

    private string? _fullName;

    public AsyncFuncBlock(Func<Parameter<VIn>, ValueTask<Parameter<VOut>>> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    public ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VIn> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return ValueTask.FromResult(new Parameter<VOut>(value.Failure)
            {
                Context = value.Context
            });

        return _func(value);
    }
}