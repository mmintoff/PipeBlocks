namespace MM.PipeBlocks.Abstractions;
/// <summary>
/// Defines a resolver for resolving instances of blocks of type <typeparamref name="X"/> that operate on a context of type <typeparamref name="C"/> and a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="C">The type of the context that the block operates on.</typeparam>
/// <typeparam name="V">The type of the value in the context.</typeparam>
public interface IBlockResolver<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Resolves an instance of a block of type <typeparamref name="X"/>.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <returns>An instance of the block of type <typeparamref name="X"/>.</returns>
    X ResolveInstance<X>() where X : IBlock<C, V>;
}
