//using MM.PipeBlocks.Abstractions;

//namespace MM.PipeBlocks.Extensions;

//public class CompositeBlock<VIn, VMiddle, VOut> : ISyncBlock<VIn, VOut>, IAsyncBlock<VIn, VOut>
//{
//    private readonly IBlock<VIn, VMiddle> _first;
//    private readonly IBlock<VMiddle, VOut> _second;

//    public CompositeBlock(IBlock<VIn, VMiddle> first, IBlock<VMiddle, VOut> second)
//    {
//        _first = first ?? throw new ArgumentNullException(nameof(first));
//        _second = second ?? throw new ArgumentNullException(nameof(second));
//    }

//    public Parameter<VOut> Execute(Parameter<VIn> value)
//    {
//        var middleResult = BlockExecutor.ExecuteSync(_first, value);
//        if (IsFinished(middleResult))
//        {
//            return new Parameter<VOut>(new CompositeFailureState<VOut>(middleResult.Match(
//                f => f,
//                _ => default!)));
//        }
//        return BlockExecutor.ExecuteSync(_second, middleResult);
//    }

//    public async ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VIn> value)
//    {
//        var middleResult = await BlockExecutor.ExecuteAsync(_first, value);
//        if (IsFinished(middleResult))
//        {
//            return new Parameter<VOut>(new CompositeFailureState<VOut>(middleResult.Match(
//                f => f,
//                _ => default!)));
//        }
//        return await BlockExecutor.ExecuteAsync(_second, middleResult);
//    }

//    private static bool IsFinished(Parameter<VMiddle> value) => value.Context.IsFlipped
//        ? !(value.Context.IsFinished || value.IsFailure)
//        : value.Context.IsFinished || value.IsFailure;
//}