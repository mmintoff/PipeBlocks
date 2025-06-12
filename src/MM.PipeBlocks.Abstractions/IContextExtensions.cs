using MM.PipeBlocks.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
public static class IContextExtensions
{
    public static void SignalBreak<V>(this IContext<V> context, IFailureState<V> failureState)
    {
        context.SignalBreak(failureState);
    }

    public static void SignalBreak<V>(this IContext<V> context)
    {
        context.SignalBreak();
    }
}