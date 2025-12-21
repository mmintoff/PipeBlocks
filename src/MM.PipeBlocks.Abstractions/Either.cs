namespace MM.PipeBlocks.Abstractions;
/// <summary>
/// Represents a value of one of two possible types (a disjoint union).
/// An instance of <see cref="Either{TL, TR}"/> is either a Left (of type TL) or a Right (of type TR).
/// </summary>
/// <typeparam name="TL">The type of the Left value.</typeparam>
/// <typeparam name="TR">The type of the Right value.</typeparam>
public class Either<TL, TR> : IEither<TL, TR>
{
    private readonly TL? _left;
    private readonly TR? _right;
    private readonly bool _isLeft;

    /// <summary>
    /// Initializes a new instance of <see cref="Either{TL, TR}"/> with a Left value.
    /// </summary>
    /// <param name="left">The value to store as Left.</param>
    public Either(TL left) => (_left, _isLeft) = (left, true);

    /// <summary>
    /// Initializes a new instance of <see cref="Either{TL, TR}"/> with a Right value.
    /// </summary>
    /// <param name="right">The value to store as Right.</param>
    public Either(TR right) => (_right, _isLeft) = (right, false);

    /// <summary>
    /// Implicitly converts a Left value to an <see cref="Either{TL, TR}"/>.
    /// </summary>
    public static implicit operator Either<TL, TR>(TL left) => new(left);

    /// <summary>
    /// Implicitly converts a Right value to an <see cref="Either{TL, TR}"/>.
    /// </summary>
    public static implicit operator Either<TL, TR>(TR right) => new(right);

    /// <summary>
    /// Executes one of the two functions depending on whether the instance holds a Left or a Right value.
    /// </summary>
    /// <typeparam name="T">The return type of the functions.</typeparam>
    /// <param name="leftFunc">Function to execute if the value is Left.</param>
    /// <param name="rightFunc">Function to execute if the value is Right.</param>
    /// <returns>The result of the executed function.</returns>
    public T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc) => _isLeft ? leftFunc(_left!) : rightFunc(_right!);

    /// <summary>
    /// Executes one of the two actions depending on whether the instance holds a Left or a Right value.
    /// </summary>
    /// <param name="leftAction">Action to execute if the value is Left.</param>
    /// <param name="rightAction">Action to execute if the value is Right.</param>
    public void Match(Action<TL> leftAction, Action<TR> rightAction)
    {
        if (_isLeft) leftAction(_left!);
        else rightAction(_right!);
    }

    /// <summary>
    /// Asynchronously executes one of the two functions depending on whether the instance holds a Left or a Right value.
    /// </summary>
    /// <param name="leftAction">Asynchronous function to execute if the value is Left.</param>
    /// <param name="rightAction">Asynchronous function to execute if the value is Right.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask MatchAsync(Func<TL, ValueTask> leftAction, Func<TR, ValueTask> rightAction)
        => _isLeft ? leftAction(_left!) : rightAction(_right!);

    /// <summary>
    /// Asynchronously executes one of the two functions with return values depending on whether the instance holds a Left or a Right value.
    /// </summary>
    /// <typeparam name="T">The return type of the functions.</typeparam>
    /// <param name="leftTask">Asynchronous function to execute if the value is Left.</param>
    /// <param name="rightTask">Asynchronous function to execute if the value is Right.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the result of the operation.</returns>
    public ValueTask<T> MatchAsync<T>(Func<TL, ValueTask<T>> leftTask, Func<TR, ValueTask<T>> rightTask)
        => _isLeft ? leftTask(_left!) : rightTask(_right!);
}
