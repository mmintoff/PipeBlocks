# MM.PipeBlocks.Abstractions
**MM.PipeBlocks.Abstractions** provides the core interfaces and types for the [MM.PipeBlocks](https://github.com/mmintoff/PipeBlocks) library, enabling modular pipelines built around safe error handling and composition.
This package is designed to be lightweight, stable, and implementation-agnostic.

## Features
- `IBlock<C, V>`: The foundational type on which all of PipeBlocks is built
- `ISyncBlock<C, V>`: Represents a synchronous block
- `IAsyncBlock<C, V>`: Represents an asynchronous block
- `PipeBlockDelegate<C, V>`: Represents a delegate of the pipeline which will call Execute
- `PipeBlockAsyncDelegate<C, V>`: Represents a delegate of the pipeline which will call ExecuteAsync
- `Either<TL, TR>`: Represents a disjoint union, necessary for operating a two-rail system
- `IContext<V>`: Represents a mutable stateful context object that will be used and referenced throughout the lifetime of a pipeline execution
- `IFailureState<V>`: Represents the failure state of a context
- `IBlockResolver<C, V>`: Represents a resolver for resolving instances of blocks; the expectation is that the implementer will wire this up with their dependency injection framework of choice