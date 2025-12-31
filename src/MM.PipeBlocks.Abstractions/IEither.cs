namespace MM.PipeBlocks.Abstractions
{
    public interface IEither<TL, TR>
    {
        void Match(Action<TL> leftAction, Action<TR> rightAction);
        T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc);
        ValueTask MatchAsync(Func<TL, ValueTask> leftAction, Func<TR, ValueTask> rightAction);
        ValueTask<T> MatchAsync<T>(Func<TL, ValueTask<T>> leftTask, Func<TR, ValueTask<T>> rightTask);
    }
}