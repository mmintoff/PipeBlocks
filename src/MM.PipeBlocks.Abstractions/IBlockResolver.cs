namespace MM.PipeBlocks.Abstractions;

public interface IBlockResolver<VIn, VOut>
{
    X ResolveInstance<X>() where X : IBlock<VIn, VOut>;
    IBlockBuilder<X, Y> CreateBlockBuilder<X, Y>();
}

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
    /// <typeparam name="X">The type of value that the block builder will build for.</typeparam>
    /// <returns>An <see cref="IBlockBuilder{X}"/> instance for building blocks for the specified value type.</returns>
    IBlockBuilder<X> CreateBlockBuilder<X>();

    /// <summary>
    /// Creates a new instance of a block builder for the specified input and output value types.
    /// </summary>
    /// <typeparam name="X">The input value type that the block builder operates on.</typeparam>
    /// <typeparam name="Y">The output value type that the block builder produces.</typeparam>
    /// <returns>
    /// An <see cref="IBlockBuilder{X, Y}"/> instance for building blocks that transform values
    /// from type <typeparamref name="X"/> to type <typeparamref name="Y"/>.
    /// </returns>
    IBlockBuilder<X, Y> CreateBlockBuilder<X, Y>();
}
