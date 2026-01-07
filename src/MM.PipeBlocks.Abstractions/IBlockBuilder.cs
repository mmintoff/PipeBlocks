using Microsoft.Extensions.Options;

namespace MM.PipeBlocks.Abstractions;

public interface IBlockBuilder<VIn, VOut>
{
    X ResolveInstance<X>() where X : IBlock<VIn, VOut>;
    IBlockBuilder<VIn2, VOut2> CreateBlockBuilder<VIn2, VOut2>();
}

public interface IBlockBuilder<V>
{
    X ResolveInstance<X>() where X : IBlock<V>;
    IBlockBuilder<V2> CreateBlockBuilder<V2>();
    IBlockBuilder<VIn2, VOut2> CreateBlockBuilder<VIn2, VOut2>();
}