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

    /// <summary>
    /// Creates a new instance of a block builder for the specified value type.
    /// </summary>
    /// <typeparam name="Y">The type of value that the block builder will build for.</typeparam>
    /// <returns>An <see cref="IBlockBuilder{Y}"/> instance for building blocks for the specified value type.</returns>
    IBlockBuilder<Y> CreateBlockBuilder<Y>();
}
