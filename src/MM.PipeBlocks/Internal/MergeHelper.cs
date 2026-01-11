using MM.PipeBlocks.Abstractions;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks.Internal;

internal static class MergeHelper
{
    internal static async ValueTask<Parameter<VOut>> AwaitAndMerge<VIn, VOut>(
        ValueTask<Parameter<VOut>> inner,
        Parameter<VIn> oldValue)
    {
        var newValue = await inner.ConfigureAwait(false);
        return Merge(newValue, oldValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Parameter<VOut> Merge<VIn, VOut>(
        Parameter<VOut> newValue,
        Parameter<VIn> oldValue)
    {
        if (oldValue.Context == newValue.Context)
            return newValue;

        oldValue.Context.Merge(newValue.Context);
        newValue.Context = oldValue.Context;
        return newValue;
    }
}