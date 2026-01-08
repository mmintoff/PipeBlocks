# MM.PipeBlocks.Abstractions
**MM.PipeBlocks.Abstractions** provides the core interfaces and types for the [MM.PipeBlocks](https://github.com/mmintoff/PipeBlocks) library, enabling modular pipelines built around safe error handling and composition.
This package is designed to be lightweight, stable, and implementation-agnostic.

## Features
- `Context`:
- `Parameter<V>`:
- `Either<TL, TR>`: Represents a disjoint union, necessary for operating a two-rail system
- `IFailureState`: Represents the failure state of a context
- `IBlock<V>`: The foundational type on which all of PipeBlocks is built
- `IBlock<VIn, VOut>`: The foundational type on which all of PipeBlocks is built
- `ISyncBlock<V>`: Represents a synchronous block
- `ISyncBlock<VIn, VOut>`: Represents a synchronous block
- `IAsyncBlock<V>`: Represents an asynchronous block
- `IAsyncBlock<VIn, VOut>`: Represents an asynchronous block
- `IBlockBuilder<V>`:
- `IBlockBuilder<VIn, VOut>`:
- `IBlockResolver<C, V>`: Represents a resolver for resolving instances of blocks; the expectation is that the implementer will wire this up with their dependency injection framework of choice