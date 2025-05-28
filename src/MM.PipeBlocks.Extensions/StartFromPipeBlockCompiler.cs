/*
 * IL Generation was initially performing better than standard execution.
 * However, due to performance improvements made to the codebase, IL generation is no longer faster.
 * Preserving here for reference alone.
 */

//using System.Reflection;
//using System.Reflection.Emit;
//using MM.PipeBlocks.Abstractions;
//using Nito.AsyncEx;
//using Microsoft.Extensions.Logging;
//using MM.PipeBlocks.Blocks;
//using System.Runtime.CompilerServices;

//namespace MM.PipeBlocks.Extensions;

//public partial class StartFromPipeBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
//    where C : IContext<V>
//{
//    private readonly Dictionary<int, Func<C, C>> _compiledSyncByIndex = new();
//    private readonly Dictionary<int, Func<C, ValueTask<C>>> _compiledAsyncByIndex = new();
//    private static readonly object _compilationLock = new object();

//    public FuncBlock<C, V> CompileSync()
//    {
//        return new FuncBlock<C, V>(c =>
//        {
//            int startIndex = _startStepFunc(c);
//            return GetCompiledSyncForIndex(startIndex)(c);
//        });
//    }

//    public AsyncFuncBlock<C, V> CompileAsync()
//    {
//        return new AsyncFuncBlock<C, V>(async c =>
//        {
//            int startIndex = _startStepFunc(c);
//            return await GetCompiledAsyncForIndex(startIndex)(c);
//        });
//    }

//    private Func<C, C> GetCompiledSyncForIndex(int startIndex)
//    {
//        // Validate bounds
//        if (startIndex < 0 || startIndex >= _blocks.Count)
//        {
//            // Return identity function for invalid indices
//            return context => context;
//        }

//        // Check if already compiled
//        if (_compiledSyncByIndex.TryGetValue(startIndex, out var existing))
//            return existing;

//        lock (_compilationLock)
//        {
//            // Double-check after acquiring lock
//            if (_compiledSyncByIndex.TryGetValue(startIndex, out existing))
//                return existing;

//            _logger.LogInformation("Compiling synchronous pipe: '{name}' starting from index: {startIndex}",
//                _pipeName, startIndex);

//            var compiled = CompileSyncFromIndex(startIndex);
//            _compiledSyncByIndex[startIndex] = compiled;

//            return compiled;
//        }
//    }

//    private Func<C, ValueTask<C>> GetCompiledAsyncForIndex(int startIndex)
//    {
//        // Validate bounds
//        if (startIndex < 0 || startIndex >= _blocks.Count)
//        {
//            // Return identity function for invalid indices
//            return context => new ValueTask<C>(context);
//        }

//        // Check if already compiled
//        if (_compiledAsyncByIndex.TryGetValue(startIndex, out var existing))
//            return existing;

//        lock (_compilationLock)
//        {
//            // Double-check after acquiring lock
//            if (_compiledAsyncByIndex.TryGetValue(startIndex, out existing))
//                return existing;

//            _logger.LogInformation("Compiling asynchronous pipe: '{name}' starting from index: {startIndex}",
//                _pipeName, startIndex);

//            var compiled = CompileAsyncFromIndex(startIndex);
//            _compiledAsyncByIndex[startIndex] = compiled;

//            return compiled;
//        }
//    }

//    private Func<C, C> CompileSyncFromIndex(int startIndex)
//    {
//        var method = new DynamicMethod(
//            $"CompiledSync_{GetHashCode():X}_From_{startIndex}",
//            typeof(C),
//            [typeof(StartFromPipeBlock<C, V>), typeof(C)],
//            typeof(StartFromPipeBlock<C, V>),
//            true);

//        var il = method.GetILGenerator();
//        var contextLocal = il.DeclareLocal(typeof(C));
//        var returnLabel = il.DefineLabel();

//        // Load context parameter and store in local
//        il.Emit(OpCodes.Ldarg_1);
//        il.Emit(OpCodes.Stloc, contextLocal);

//        // Log: "Executing compiled pipe: '{name}' synchronously from index: {StartIndex} for context: {CorrelationId}"
//        EmitLogExecutingPipeFromIndex(il, contextLocal, startIndex, true);

//        // Unroll blocks from startIndex to end
//        for (int i = startIndex; i < _blocks.Count; i++)
//        {
//            var block = _blocks[i];
//            var nextBlockLabel = il.DefineLabel();
//            var syncLabel = il.DefineLabel();
//            var asyncLabel = il.DefineLabel();
//            var blockLocal = il.DeclareLocal(typeof(IBlock<C, V>));

//            // Check if context is finished
//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Call, typeof(StartFromPipeBlock<C, V>)
//                .GetMethod("IsFinished", BindingFlags.NonPublic | BindingFlags.Static)
//                ?? throw new InvalidOperationException("IsFinished(C) method not found"));

//            // If finished, log stopping and jump to return
//            var continueLabel = il.DefineLabel();
//            il.Emit(OpCodes.Brfalse, continueLabel);

//            // Log: "Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}"
//            EmitLogStoppingPipe(il, contextLocal, i, true);
//            il.Emit(OpCodes.Br, returnLabel);

//            il.MarkLabel(continueLabel);

//            // Load block instance from _blocks collection
//            il.Emit(OpCodes.Ldarg_0); // Load StartFromPipeBlock instance
//            il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_blocks",
//                BindingFlags.NonPublic | BindingFlags.Instance) ??
//                throw new InvalidOperationException("_blocks field not found"));
//            il.Emit(OpCodes.Ldc_I4, i); // Load index
//            il.Emit(OpCodes.Callvirt, typeof(IList<IBlock<C, V>>).GetProperty("Item")?.GetGetMethod() ??
//                throw new InvalidOperationException("Item indexer not found"));
//            il.Emit(OpCodes.Stloc, blockLocal);

//            // Check if block implements ISyncBlock
//            il.Emit(OpCodes.Ldloc, blockLocal);
//            il.Emit(OpCodes.Isinst, typeof(ISyncBlock<C, V>));
//            il.Emit(OpCodes.Brtrue, syncLabel);

//            // If not ISyncBlock, jump to async path
//            il.Emit(OpCodes.Br, asyncLabel);

//            // Sync path
//            il.MarkLabel(syncLabel);
//            il.Emit(OpCodes.Ldloc, blockLocal);
//            il.Emit(OpCodes.Castclass, typeof(ISyncBlock<C, V>));
//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Callvirt, typeof(ISyncBlock<C, V>).GetMethod("Execute") ??
//                throw new InvalidOperationException("Execute method not found"));
//            il.Emit(OpCodes.Stloc, contextLocal);
//            il.Emit(OpCodes.Br, nextBlockLabel);

//            // Async path - execute async block synchronously
//            il.MarkLabel(asyncLabel);
//            il.Emit(OpCodes.Ldloc, blockLocal);
//            il.Emit(OpCodes.Castclass, typeof(IAsyncBlock<C, V>));
//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Call, typeof(StartFromPipeBlock<C, V>).GetMethod("RunAsyncBlockSync",
//                BindingFlags.NonPublic | BindingFlags.Static) ??
//                throw new InvalidOperationException("RunAsyncBlockSync not found"));
//            il.Emit(OpCodes.Stloc, contextLocal);

//            // Continue to next block
//            il.MarkLabel(nextBlockLabel);
//        }

//        // Return label - return final context
//        il.MarkLabel(returnLabel);

//        // Log: "Completed synchronous pipe: '{name}' execution for context: {CorrelationId}"
//        EmitLogCompletedPipe(il, contextLocal, true);

//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Ret);

//        var compiledDelegate = (Func<StartFromPipeBlock<C, V>, C, C>)method.CreateDelegate(typeof(Func<StartFromPipeBlock<C, V>, C, C>));
//        var result = (Func<C, C>)((context) => compiledDelegate(this, context));

//        _logger.LogInformation("Synchronous pipe: '{name}' from index: {startIndex} compiled", _pipeName, startIndex);

//        return result;
//    }

//    private Func<C, ValueTask<C>> CompileAsyncFromIndex(int startIndex)
//    {
//        var method = new DynamicMethod(
//            $"CompiledAsync_{GetHashCode():X}_From_{startIndex}",
//            typeof(ValueTask<C>),
//            [typeof(StartFromPipeBlock<C, V>), typeof(C)],
//            typeof(StartFromPipeBlock<C, V>),
//            true);

//        var il = method.GetILGenerator();
//        var contextLocal = il.DeclareLocal(typeof(C));
//        var blockLocal = il.DeclareLocal(typeof(IBlock<C, V>));
//        var taskLocal = il.DeclareLocal(typeof(ValueTask<C>));
//        var awaiterLocal = il.DeclareLocal(typeof(System.Runtime.CompilerServices.ValueTaskAwaiter<C>));
//        var returnLabel = il.DefineLabel();

//        // Load context parameter and store in local
//        il.Emit(OpCodes.Ldarg_1);
//        il.Emit(OpCodes.Stloc, contextLocal);

//        // Log: "Executing compiled pipe: '{name}' asynchronously from index: {StartIndex} for context: {CorrelationId}"
//        EmitLogExecutingPipeFromIndex(il, contextLocal, startIndex, false);

//        // Unroll blocks from startIndex to end
//        for (int i = startIndex; i < _blocks.Count; i++)
//        {
//            var block = _blocks[i];
//            var nextBlockLabel = il.DefineLabel();
//            var asyncLabel = il.DefineLabel();
//            var syncLabel = il.DefineLabel();

//            // Check if context is finished
//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Call, typeof(StartFromPipeBlock<C, V>)
//                .GetMethod("IsFinished", BindingFlags.NonPublic | BindingFlags.Static)
//                ?? throw new InvalidOperationException("IsFinished(C) method not found"));

//            // If finished, log stopping and jump to return
//            var continueLabel = il.DefineLabel();
//            il.Emit(OpCodes.Brfalse, continueLabel);

//            // Log: "Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}"
//            EmitLogStoppingPipe(il, contextLocal, i, false);
//            il.Emit(OpCodes.Br, returnLabel);

//            il.MarkLabel(continueLabel);

//            // Load block instance from _blocks collection
//            il.Emit(OpCodes.Ldarg_0); // Load StartFromPipeBlock instance
//            il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_blocks",
//                BindingFlags.NonPublic | BindingFlags.Instance) ??
//                throw new InvalidOperationException("_blocks field not found"));
//            il.Emit(OpCodes.Ldc_I4, i); // Load index
//            il.Emit(OpCodes.Callvirt, typeof(IList<IBlock<C, V>>).GetProperty("Item")?.GetGetMethod() ??
//                throw new InvalidOperationException("Item indexer not found"));
//            il.Emit(OpCodes.Stloc, blockLocal);

//            // Check if block implements IAsyncBlock
//            il.Emit(OpCodes.Ldloc, blockLocal);
//            il.Emit(OpCodes.Isinst, typeof(IAsyncBlock<C, V>));
//            il.Emit(OpCodes.Brtrue, asyncLabel);

//            // If not IAsyncBlock, jump to sync path
//            il.Emit(OpCodes.Br, syncLabel);

//            // Async path
//            il.MarkLabel(asyncLabel);
//            il.Emit(OpCodes.Ldloc, blockLocal);
//            il.Emit(OpCodes.Castclass, typeof(IAsyncBlock<C, V>));
//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Callvirt, typeof(IAsyncBlock<C, V>).GetMethod("ExecuteAsync") ??
//                throw new InvalidOperationException("ExecuteAsync method not found"));
//            il.Emit(OpCodes.Stloc, taskLocal);

//            // Get awaiter and get result
//            il.Emit(OpCodes.Ldloca, taskLocal);
//            il.Emit(OpCodes.Call, typeof(ValueTask<C>).GetMethod("GetAwaiter") ??
//                throw new InvalidOperationException("GetAwaiter method not found"));
//            il.Emit(OpCodes.Stloc, awaiterLocal);

//            il.Emit(OpCodes.Ldloca, awaiterLocal);
//            il.Emit(OpCodes.Call, typeof(System.Runtime.CompilerServices.ValueTaskAwaiter<C>).GetMethod("GetResult") ??
//                throw new InvalidOperationException("GetResult method not found"));
//            il.Emit(OpCodes.Stloc, contextLocal);
//            il.Emit(OpCodes.Br, nextBlockLabel);

//            // Sync path - wrap sync execution in ValueTask
//            il.MarkLabel(syncLabel);
//            il.Emit(OpCodes.Ldloc, blockLocal);
//            il.Emit(OpCodes.Castclass, typeof(ISyncBlock<C, V>));
//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Callvirt, typeof(ISyncBlock<C, V>).GetMethod("Execute") ??
//                throw new InvalidOperationException("Execute method not found"));
//            il.Emit(OpCodes.Stloc, contextLocal);

//            // Continue to next block
//            il.MarkLabel(nextBlockLabel);
//        }

//        // Return label - return final context wrapped in ValueTask
//        il.MarkLabel(returnLabel);

//        EmitLogCompletedPipe(il, contextLocal, false);

//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Newobj, typeof(ValueTask<C>).GetConstructor(new[] { typeof(C) }) ??
//            throw new InvalidOperationException("ValueTask<C> constructor not found"));
//        il.Emit(OpCodes.Ret);

//        var compiledDelegate = (Func<StartFromPipeBlock<C, V>, C, ValueTask<C>>)method.CreateDelegate(typeof(Func<StartFromPipeBlock<C, V>, C, ValueTask<C>>));
//        var result = (Func<C, ValueTask<C>>)((context) => compiledDelegate(this, context));

//        _logger.LogInformation("Asynchronous pipe: '{name}' from index: {startIndex} compiled", _pipeName, startIndex);

//        return result;
//    }

//    // Convenience method to precompile all possible start indices
//    public void PrecompileAllIndices()
//    {
//        _logger.LogInformation("Precompiling all start indices for pipe: '{name}'", _pipeName);

//        for (int i = 0; i < _blocks.Count; i++)
//        {
//            GetCompiledSyncForIndex(i);
//            GetCompiledAsyncForIndex(i);
//        }

//        _logger.LogInformation("Precompilation completed for pipe: '{name}' - {count} sync and {count} async variants",
//            _pipeName, _blocks.Count, _blocks.Count);
//    }

//    private static void EmitLogExecutingPipeFromIndex(ILGenerator il, LocalBuilder contextLocal, int startIndex, bool isSync)
//    {
//        // Load logger
//        il.Emit(OpCodes.Ldarg_0); // Load StartFromPipeBlock instance
//        il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_logger",
//            BindingFlags.NonPublic | BindingFlags.Instance) ??
//            throw new InvalidOperationException("_logger field not found"));

//        // Load message template
//        var messageTemplate = isSync
//            ? "Executing compiled pipe: '{name}' synchronously from index: {StartIndex} for context: {CorrelationId}"
//            : "Executing compiled pipe: '{name}' asynchronously from index: {StartIndex} for context: {CorrelationId}";
//        il.Emit(OpCodes.Ldstr, messageTemplate);

//        // Create object array for parameters
//        il.Emit(OpCodes.Ldc_I4_3); // Array length = 3
//        il.Emit(OpCodes.Newarr, typeof(object));

//        // Set array[0] = pipeName
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_0);
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_pipeName",
//            BindingFlags.NonPublic | BindingFlags.Instance) ??
//            throw new InvalidOperationException("_pipeName field not found"));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Set array[1] = startIndex (compile-time constant)
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_1);
//        il.Emit(OpCodes.Ldc_I4, startIndex);
//        il.Emit(OpCodes.Box, typeof(int));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Set array[2] = correlationId
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_2);
//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Callvirt, typeof(IContext<V>).GetProperty("CorrelationId")?.GetGetMethod() ??
//            throw new InvalidOperationException("CorrelationId property not found"));
//        il.Emit(OpCodes.Box, typeof(Guid));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Call LoggerExtensions.LogTrace(ILogger, string, object[])
//        il.Emit(OpCodes.Call, typeof(LoggerExtensions).GetMethod("LogTrace",
//            new[] { typeof(ILogger), typeof(string), typeof(object[]) }) ??
//            throw new InvalidOperationException("LoggerExtensions.LogTrace method not found"));
//    }

//    private static void EmitLogStoppingPipe(ILGenerator il, LocalBuilder contextLocal, int stepIndex, bool isSync)
//    {
//        // Load logger
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_logger",
//            BindingFlags.NonPublic | BindingFlags.Instance) ??
//            throw new InvalidOperationException("_logger field not found"));

//        // Load message template
//        var messageTemplate = isSync
//            ? "Stopping synchronous compiled pipe: '{name}' execution at step: {Step} for context: {CorrelationId}"
//            : "Stopping asynchronous compiled pipe: '{name}' execution at step: {Step} for context: {CorrelationId}";
//        il.Emit(OpCodes.Ldstr, messageTemplate);

//        // Create object array for parameters
//        il.Emit(OpCodes.Ldc_I4_3); // Array length = 3
//        il.Emit(OpCodes.Newarr, typeof(object));

//        // Set array[0] = pipeName
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_0);
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_pipeName",
//            BindingFlags.NonPublic | BindingFlags.Instance) ??
//            throw new InvalidOperationException("_pipeName field not found"));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Set array[1] = stepIndex (compile-time constant)
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_1);
//        il.Emit(OpCodes.Ldc_I4, stepIndex);
//        il.Emit(OpCodes.Box, typeof(int));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Set array[2] = correlationId
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_2);
//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Callvirt, typeof(IContext<V>).GetProperty("CorrelationId")?.GetGetMethod() ??
//            throw new InvalidOperationException("CorrelationId property not found"));
//        il.Emit(OpCodes.Box, typeof(Guid));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Call LoggerExtensions.LogTrace(ILogger, string, object[])
//        il.Emit(OpCodes.Call, typeof(LoggerExtensions).GetMethod("LogTrace",
//            new[] { typeof(ILogger), typeof(string), typeof(object[]) }) ??
//            throw new InvalidOperationException("LoggerExtensions.LogTrace method not found"));
//    }

//    private static void EmitLogCompletedPipe(ILGenerator il, LocalBuilder contextLocal, bool isSync)
//    {
//        // Load logger
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_logger",
//            BindingFlags.NonPublic | BindingFlags.Instance) ??
//            throw new InvalidOperationException("_logger field not found"));

//        var messageTemplate = isSync
//            ? "Completed synchronous compiled pipe: '{name}' execution for context: {CorrelationId}"
//            : "Completed asynchronous compiled pipe: '{name}' execution for context: {CorrelationId}";
//        il.Emit(OpCodes.Ldstr, messageTemplate);

//        // Create object array for parameters
//        il.Emit(OpCodes.Ldc_I4_2); // Array length = 2
//        il.Emit(OpCodes.Newarr, typeof(object));

//        // Set array[0] = pipeName
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_0);
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(StartFromPipeBlock<C, V>).GetField("_pipeName",
//            BindingFlags.NonPublic | BindingFlags.Instance) ??
//            throw new InvalidOperationException("_pipeName field not found"));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Set array[1] = correlationId
//        il.Emit(OpCodes.Dup);
//        il.Emit(OpCodes.Ldc_I4_1);
//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Callvirt, typeof(IContext<V>).GetProperty("CorrelationId")?.GetGetMethod() ??
//            throw new InvalidOperationException("CorrelationId property not found"));
//        il.Emit(OpCodes.Box, typeof(Guid));
//        il.Emit(OpCodes.Stelem_Ref);

//        // Call LoggerExtensions.LogTrace(ILogger, string, object[])
//        il.Emit(OpCodes.Call, typeof(LoggerExtensions).GetMethod("LogTrace",
//            [typeof(ILogger), typeof(string), typeof(object[])]) ??
//            throw new InvalidOperationException("LoggerExtensions.LogTrace method not found"));
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static C RunAsyncBlockSync(IAsyncBlock<C, V> asyncBlock, C context)
//        => ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(context));

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static C ExecuteValueTaskSynchronously(ValueTask<C> task)
//        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
//}