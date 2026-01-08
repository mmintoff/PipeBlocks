using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;

/// <summary>
/// Configures a mapping from <typeparamref name="V"/> to <typeparamref name="VNext"/>.
/// </summary>
/// <typeparam name="V">The input type.</typeparam>
/// <typeparam name="VNext">The output type.</typeparam>
public class Mapper<V, VNext>
{
    private BlockBuilder<V> _blockBuilder;
    private BlockBuilder<VNext> _nextBlockBuilder;
    private IPipeBlock<V> _pipe;

    internal Mapper(IPipeBlock<V> pipe, BlockBuilder<V> blockBuilder)
    {
        _pipe = pipe;
        _blockBuilder = blockBuilder;
        _nextBlockBuilder = (BlockBuilder<VNext>)blockBuilder.CreateBlockBuilder<VNext>();
    }

    /// <summary>
    /// Specifies the block to perform the mapping, resolved from DI.
    /// </summary>
    public MapPipeBlock<V, V, VNext> Via<X>()
        where X : IBlock<V, VNext>
        => new(_pipe, _blockBuilder.CreateBlockBuilder<V, VNext>().ResolveInstance<X>(), _nextBlockBuilder);

    /// <summary>
    /// Specifies the block instance to perform the mapping.
    /// </summary>
    public MapPipeBlock<V, V, VNext> Via(IBlock<V, VNext> block)
        => new(_pipe, block, _nextBlockBuilder);

    /// <summary>
    /// Specifies a factory function to create the mapping block.
    /// </summary>
    public MapPipeBlock<V, V, VNext> Via(Func<BlockBuilder<V, VNext>, IBlock<V, VNext>> func)
        => new(_pipe, func((BlockBuilder<V, VNext>)_blockBuilder.CreateBlockBuilder<V, VNext>()), _nextBlockBuilder);
}

/// <summary>
/// Configures a mapping from <typeparamref name="VOut"/> to <typeparamref name="VNext"/>.
/// </summary>
/// <typeparam name="VRoot">The root input type of the pipeline.</typeparam>
/// <typeparam name="VOut">The input type for this mapping.</typeparam>
/// <typeparam name="VNext">The output type.</typeparam>
public class Mapper<VRoot, VOut, VNext>
{
    private BlockBuilder<VOut> _blockBuilder;
    private BlockBuilder<VNext> _nextBlockBuilder;
    private IPipeBlock<VRoot, VOut> _pipe;

    internal Mapper(IPipeBlock<VRoot, VOut> pipe, BlockBuilder<VOut> blockBuilder)
    {
        _pipe = pipe;
        _blockBuilder = blockBuilder;
        _nextBlockBuilder = (BlockBuilder<VNext>)blockBuilder.CreateBlockBuilder<VNext>();
    }

    /// <summary>
    /// Specifies the block to perform the mapping, resolved from DI.
    /// </summary>
    public MapPipeBlock<VRoot, VOut, VNext> Via<X>()
        where X : IBlock<VOut, VNext>
        => new(_pipe, _blockBuilder.CreateBlockBuilder<VOut, VNext>().ResolveInstance<X>(), _nextBlockBuilder);

    /// <summary>
    /// Specifies the block instance to perform the mapping.
    /// </summary>
    public MapPipeBlock<VRoot, VOut, VNext> Via(IBlock<VOut, VNext> block)
        => new(_pipe, block, _nextBlockBuilder);

    /// <summary>
    /// Specifies a factory function to create the mapping block.
    /// </summary>
    public MapPipeBlock<VRoot, VOut, VNext> Via(Func<BlockBuilder<VOut, VNext>, IBlock<VOut, VNext>> func)
        => new(_pipe, func((BlockBuilder<VOut, VNext>)_blockBuilder.CreateBlockBuilder<VOut, VNext>()), _nextBlockBuilder);
}