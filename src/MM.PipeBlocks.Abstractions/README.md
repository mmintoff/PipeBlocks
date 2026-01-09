# MM.PipeBlocks.Abstractions
**MM.PipeBlocks.Abstractions** provides the core interfaces and types for the [MM.PipeBlocks](https://github.com/mmintoff/PipeBlocks) library, enabling modular pipelines built around safe error handling and composition.
This package is designed to be lightweight, stable, and implementation-agnostic.

## Features
- `Context`: High-performance key-value store that flows through the entire pipeline, preserving correlation Ids, execution state, and custom data across all blocks and type transformations
- `Parameter<V>`: A wrapper combining your data value with execution context and failure state, implementing the Either monad pattern to represent success (right rail) or failure (left rail)
- `Either<TL, TR>`: Represents a disjoint union, necessary for operating a two-rail system where execution flows alone either the success path or the failure path
- `IFailureState`: Represents the failure state of a parameter, containing the input value that caused the failure and diagnostic information
- `IBlock<V>`: The foundational marker interface for homogeneous blocks where input and output types are the same, serving as the base for all block types in PipeBlocks
- `IBlock<VIn, VOut>`: The foundational marker interface for hetereogeneous blocks that transform from one type to another, enabling type-safe data transformation through PipeBlocks
- `ISyncBlock<V>`: Represents a synchronous block with homogeneous input/output types, executing logic in a single-threaded non-async manner
- `ISyncBlock<VIn, VOut>`: Represents a synchronous block that transforms from `VIn` to `VOut`, executing type transformations without async overhead
- `IAsyncBlock<V>`: Represents an asynchronous block with homogeneous input/output types, supporting async/await patterns for I/O-bound or long-running operations
- `IAsyncBlock<VIn, VOut>`: Represents an asynchronous block that transforms from `VIn` to `VOut`, enabling async type transformations with proper resource management
- `IBlockBuilder<V>`: Provides factory methods for creating and resolving homogeneous blocks, supporting fluent pipeline construction with dependency injection integration
- `IBlockBuilder<VIn, VOut>`: Provides factory methods for creating and resolving heterogeneous transformation blocks, enabling type-safe block resolution with dependency injection
- `IBlockResolver<C, V>`: Represents a resolver for resolving instances of blocks from a container (typically dependency injection); implementers wire this up with their DI framework of choice to enable automatic block instantion in PipeBlocks