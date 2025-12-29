namespace MM.PipeBlocks.Abstractions;
/// <summary>
/// Defines a resolver for resolving instances of blocks that operate on a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="V">The type of the value in the parameter.</typeparam>
public interface IBlockResolver<V>
{
    /// <summary>
    /// Resolves an instance of a block of type <typeparamref name="X"/>.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <returns>An instance of the block of type <typeparamref name="X"/>.</returns>
    X ResolveInstance<X>() where X : IBlock<V>;
}
