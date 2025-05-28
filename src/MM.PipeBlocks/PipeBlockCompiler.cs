/*
 * IL Generation was initially performing better than standard execution.
 * However, due to performance improvements made to the codebase, IL generation is no longer faster.
 * Preserving here for reference alone.
 */

//using System.Reflection;
//using System.Reflection.Emit;
//using Microsoft.Extensions.Logging;
//using System.Runtime.CompilerServices;
//using Nito.AsyncEx;
//using MM.PipeBlocks.Abstractions;
//using MM.PipeBlocks.Blocks;

//namespace MM.PipeBlocks;

//public partial class PipeBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
//    where C : IContext<V>
//{
//    private FuncBlock<C, V>? _compiledFuncBlock;
//    private AsyncFuncBlock<C, V>? _compiledAsyncFuncBlock;

//    private static readonly object _compilationLock = new object();

//    public FuncBlock<C, V> CompileSync()
//    {
//        if (_compiledFuncBlock != null)
//            return _compiledFuncBlock;

//        lock (_compilationLock)
//        {
//            if (_compiledFuncBlock != null)
//                return _compiledFuncBlock;

//            _logger.LogInformation("Compiling synchronous pipe: '{name}'", _pipeName);
//            Func<C, C>? compiledFunc;

//            var method = new DynamicMethod(
//                $"CompiledSync_{GetHashCode():X}",
//                typeof(C),
//                [typeof(PipeBlock<C, V>), typeof(C)],
//                typeof(PipeBlock<C, V>),
//                true);

//            var il = method.GetILGenerator();
//            var contextLocal = il.DeclareLocal(typeof(C));
//            var returnLabel = il.DefineLabel();

//            // Load context parameter and store in local
//            il.Emit(OpCodes.Ldarg_1);
//            il.Emit(OpCodes.Stloc, contextLocal);

//            // Log: "Executing pipe: '{name}' synchronously for context: {CorrelationId}"
//            EmitLogExecutingPipe(il, contextLocal, true);

//            for (int i = 0; i < _blocks.Count; i++)
//            {
//                var block = _blocks[i];
//                var nextBlockLabel = il.DefineLabel();
//                var syncLabel = il.DefineLabel();
//                var asyncLabel = il.DefineLabel();
//                var blockLocal = il.DeclareLocal(typeof(IBlock<C, V>));

//                // Check if context is finished
//                il.Emit(OpCodes.Ldloc, contextLocal);
//                il.Emit(OpCodes.Call, typeof(PipeBlock<C, V>)
//                    .GetMethod("IsFinished", BindingFlags.NonPublic | BindingFlags.Static)
//                    ?? throw new InvalidOperationException("IsFinished(C) method not found"));

//                // If finished, log stopping and jump to return
//                var continueLabel = il.DefineLabel();
//                il.Emit(OpCodes.Brfalse, continueLabel);

//                // Log: "Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}"
//                EmitLogStoppingPipe(il, contextLocal, i, true);
//                il.Emit(OpCodes.Br, returnLabel);

//                il.MarkLabel(continueLabel);

//                // Load block instance from _blocks collection
//                il.Emit(OpCodes.Ldarg_0); // Load PipeBlock instance
//                il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_blocks",
//                    BindingFlags.NonPublic | BindingFlags.Instance) ??
//                    throw new InvalidOperationException("_blocks field not found"));
//                il.Emit(OpCodes.Ldc_I4, i); // Load index
//                il.Emit(OpCodes.Callvirt, typeof(IList<IBlock<C, V>>).GetProperty("Item")?.GetGetMethod() ??
//                    throw new InvalidOperationException("Item indexer not found"));
//                il.Emit(OpCodes.Stloc, blockLocal);

//                // Check if block implements ISyncBlock
//                il.Emit(OpCodes.Ldloc, blockLocal);
//                il.Emit(OpCodes.Isinst, typeof(ISyncBlock<C, V>));
//                il.Emit(OpCodes.Brtrue, syncLabel);

//                // If not ISyncBlock, jump to async path
//                il.Emit(OpCodes.Br, asyncLabel);

//                // Sync path
//                il.MarkLabel(syncLabel);
//                il.Emit(OpCodes.Ldloc, blockLocal);
//                il.Emit(OpCodes.Castclass, typeof(ISyncBlock<C, V>));
//                il.Emit(OpCodes.Ldloc, contextLocal);
//                il.Emit(OpCodes.Callvirt, typeof(ISyncBlock<C, V>).GetMethod("Execute") ??
//                    throw new InvalidOperationException("Execute method not found"));
//                il.Emit(OpCodes.Stloc, contextLocal);
//                il.Emit(OpCodes.Br, nextBlockLabel);

//                // Async path - execute async block synchronously
//                il.MarkLabel(asyncLabel);
//                il.Emit(OpCodes.Ldloc, blockLocal);
//                il.Emit(OpCodes.Castclass, typeof(IAsyncBlock<C, V>));
//                il.Emit(OpCodes.Ldloc, contextLocal);
//                il.Emit(OpCodes.Call, typeof(PipeBlock<C, V>).GetMethod("RunAsyncBlockSync",
//                    BindingFlags.NonPublic | BindingFlags.Static) ??
//                    throw new InvalidOperationException("RunAsyncBlockSync not found"));
//                il.Emit(OpCodes.Stloc, contextLocal);

//                // Continue to next block
//                il.MarkLabel(nextBlockLabel);
//            }

//            // Return label - return final context
//            il.MarkLabel(returnLabel);

//            // Log: "Completed synchronous pipe: '{name}' execution for context: {CorrelationId}"
//            EmitLogCompletedPipe(il, contextLocal, true);

//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Ret);

//            var compiledDelegate = (Func<PipeBlock<C, V>, C, C>)method.CreateDelegate(typeof(Func<PipeBlock<C, V>, C, C>));
//            compiledFunc = (context) => compiledDelegate(this, context);

//            _logger.LogInformation("Synchronous pipe: '{name}' compiled", _pipeName);

//            _compiledFuncBlock = new FuncBlock<C, V>(compiledFunc);
//            return _compiledFuncBlock;
//        }
//    }

//    public AsyncFuncBlock<C, V> CompileAsync()
//    {
//        if (_compiledAsyncFuncBlock != null)
//            return _compiledAsyncFuncBlock;

//        lock (_compilationLock)
//        {
//            if (_compiledAsyncFuncBlock != null)
//                return _compiledAsyncFuncBlock;

//            _logger.LogInformation("Compiling asynchronous pipe: '{name}'", _pipeName);
//            Func<C, ValueTask<C>>? compiledFunc;

//            var method = new DynamicMethod(
//                $"CompiledAsync_{GetHashCode():X}",
//                typeof(ValueTask<C>),
//                [typeof(PipeBlock<C, V>), typeof(C)],
//                typeof(PipeBlock<C, V>),
//                true);

//            var il = method.GetILGenerator();
//            var contextLocal = il.DeclareLocal(typeof(C));
//            var blockLocal = il.DeclareLocal(typeof(IBlock<C, V>));
//            var taskLocal = il.DeclareLocal(typeof(ValueTask<C>));
//            var awaiterLocal = il.DeclareLocal(typeof(ValueTaskAwaiter<C>));
//            var returnLabel = il.DefineLabel();

//            // Load context parameter and store in local
//            il.Emit(OpCodes.Ldarg_1);
//            il.Emit(OpCodes.Stloc, contextLocal);

//            // Log: "Executing pipe: '{name}' asynchronously for context: {CorrelationId}"
//            EmitLogExecutingPipe(il, contextLocal, false);

//            for (int i = 0; i < _blocks.Count; i++)
//            {
//                var block = _blocks[i];
//                var nextBlockLabel = il.DefineLabel();
//                var asyncLabel = il.DefineLabel();
//                var syncLabel = il.DefineLabel();

//                // Check if context is finished
//                il.Emit(OpCodes.Ldloc, contextLocal);
//                il.Emit(OpCodes.Call, typeof(PipeBlock<C, V>)
//                    .GetMethod("IsFinished", BindingFlags.NonPublic | BindingFlags.Static)
//                    ?? throw new InvalidOperationException("IsFinished(C) method not found"));

//                // If finished, log stopping and jump to return
//                var continueLabel = il.DefineLabel();
//                il.Emit(OpCodes.Brfalse, continueLabel);

//                // Log: "Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}"
//                EmitLogStoppingPipe(il, contextLocal, i, false);
//                il.Emit(OpCodes.Br, returnLabel);

//                il.MarkLabel(continueLabel);

//                // Load block instance from _blocks collection
//                il.Emit(OpCodes.Ldarg_0); // Load PipeBlock instance
//                il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_blocks",
//                    BindingFlags.NonPublic | BindingFlags.Instance) ??
//                    throw new InvalidOperationException("_blocks field not found"));
//                il.Emit(OpCodes.Ldc_I4, i); // Load index
//                il.Emit(OpCodes.Callvirt, typeof(IList<IBlock<C, V>>).GetProperty("Item")?.GetGetMethod() ??
//                    throw new InvalidOperationException("Item indexer not found"));
//                il.Emit(OpCodes.Stloc, blockLocal);

//                // Check if block implements IAsyncBlock
//                il.Emit(OpCodes.Ldloc, blockLocal);
//                il.Emit(OpCodes.Isinst, typeof(IAsyncBlock<C, V>));
//                il.Emit(OpCodes.Brtrue, asyncLabel);

//                // If not IAsyncBlock, jump to sync path
//                il.Emit(OpCodes.Br, syncLabel);

//                // Async path
//                il.MarkLabel(asyncLabel);
//                il.Emit(OpCodes.Ldloc, blockLocal);
//                il.Emit(OpCodes.Castclass, typeof(IAsyncBlock<C, V>));
//                il.Emit(OpCodes.Ldloc, contextLocal);
//                il.Emit(OpCodes.Callvirt, typeof(IAsyncBlock<C, V>).GetMethod("ExecuteAsync") ??
//                    throw new InvalidOperationException("ExecuteAsync method not found"));
//                il.Emit(OpCodes.Stloc, taskLocal);

//                // Get awaiter and get result
//                il.Emit(OpCodes.Ldloca, taskLocal);
//                il.Emit(OpCodes.Call, typeof(ValueTask<C>).GetMethod("GetAwaiter") ??
//                    throw new InvalidOperationException("GetAwaiter method not found"));
//                il.Emit(OpCodes.Stloc, awaiterLocal);

//                il.Emit(OpCodes.Ldloca, awaiterLocal);
//                il.Emit(OpCodes.Call, typeof(ValueTaskAwaiter<C>).GetMethod("GetResult") ??
//                    throw new InvalidOperationException("GetResult method not found"));
//                il.Emit(OpCodes.Stloc, contextLocal);
//                il.Emit(OpCodes.Br, nextBlockLabel);

//                // Sync path - wrap sync execution in ValueTask
//                il.MarkLabel(syncLabel);
//                il.Emit(OpCodes.Ldloc, blockLocal);
//                il.Emit(OpCodes.Castclass, typeof(ISyncBlock<C, V>));
//                il.Emit(OpCodes.Ldloc, contextLocal);
//                il.Emit(OpCodes.Callvirt, typeof(ISyncBlock<C, V>).GetMethod("Execute") ??
//                    throw new InvalidOperationException("Execute method not found"));
//                il.Emit(OpCodes.Stloc, contextLocal);

//                // Continue to next block
//                il.MarkLabel(nextBlockLabel);
//            }

//            // Return label - return final context wrapped in ValueTask
//            il.MarkLabel(returnLabel);

//            EmitLogCompletedPipe(il, contextLocal, false);

//            il.Emit(OpCodes.Ldloc, contextLocal);
//            il.Emit(OpCodes.Newobj, typeof(ValueTask<C>).GetConstructor(new[] { typeof(C) }) ??
//                throw new InvalidOperationException("ValueTask<C> constructor not found"));
//            il.Emit(OpCodes.Ret);

//            var compiledDelegate = (Func<PipeBlock<C, V>, C, ValueTask<C>>)method.CreateDelegate(typeof(Func<PipeBlock<C, V>, C, ValueTask<C>>));
//            compiledFunc = (context) => compiledDelegate(this, context);

//            _logger.LogInformation("Asynchronous pipe: '{name}' compiled", _pipeName);

//            _compiledAsyncFuncBlock = new AsyncFuncBlock<C, V>(compiledFunc);
//            return _compiledAsyncFuncBlock;
//        }
//    }

//    // Simplified emit methods that use the pre-compiled delegates
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void EmitLogExecutingPipe(ILGenerator il, LocalBuilder contextLocal, bool isSync)
//    {
//        var delegateField = isSync ? "s_sync_logExecutingPipe" : "s_async_logExecutingPipe";

//        // Load the pre-compiled delegate
//        il.Emit(OpCodes.Ldsfld, typeof(PipeBlock<C, V>).GetField(delegateField, BindingFlags.NonPublic | BindingFlags.Static)
//            ?? throw new InvalidOperationException($"{delegateField} field not found"));

//        // Load logger
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?? throw new InvalidOperationException("_logger field not found"));

//        // Load pipe name
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_pipeName", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?? throw new InvalidOperationException("_pipeName field not found"));

//        // Load correlation ID
//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Callvirt, typeof(IContext<V>).GetProperty("CorrelationId")?.GetGetMethod()
//            ?? throw new InvalidOperationException("CorrelationId property not found"));

//        // Load null exception
//        il.Emit(OpCodes.Ldnull);

//        // Invoke delegate
//        il.Emit(OpCodes.Callvirt, typeof(Action<ILogger, string, Guid, Exception?>).GetMethod("Invoke")
//            ?? throw new InvalidOperationException("Action.Invoke method not found"));
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void EmitLogStoppingPipe(ILGenerator il, LocalBuilder contextLocal, int stepIndex, bool isSync)
//    {
//        var delegateField = isSync ? "s_sync_logStoppingPipe" : "s_async_logStoppingPipe";

//        // Load the pre-compiled delegate
//        il.Emit(OpCodes.Ldsfld, typeof(PipeBlock<C, V>).GetField(delegateField, BindingFlags.NonPublic | BindingFlags.Static)
//            ?? throw new InvalidOperationException($"{delegateField} field not found"));

//        // Load logger
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?? throw new InvalidOperationException("_logger field not found"));

//        // Load pipe name
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_pipeName", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?? throw new InvalidOperationException("_pipeName field not found"));

//        // Load step index
//        il.Emit(OpCodes.Ldc_I4, stepIndex);

//        // Load correlation ID
//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Callvirt, typeof(IContext<V>).GetProperty("CorrelationId")?.GetGetMethod()
//            ?? throw new InvalidOperationException("CorrelationId property not found"));

//        // Load null exception
//        il.Emit(OpCodes.Ldnull);

//        // Invoke delegate
//        il.Emit(OpCodes.Callvirt, typeof(Action<ILogger, string, int, Guid, Exception?>).GetMethod("Invoke")
//            ?? throw new InvalidOperationException("Action.Invoke method not found"));
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void EmitLogCompletedPipe(ILGenerator il, LocalBuilder contextLocal, bool isSync)
//    {
//        var delegateField = isSync ? "s_sync_logCompletedPipe" : "s_async_logCompletedPipe";

//        // Load the pre-compiled delegate
//        il.Emit(OpCodes.Ldsfld, typeof(PipeBlock<C, V>).GetField(delegateField, BindingFlags.NonPublic | BindingFlags.Static)
//            ?? throw new InvalidOperationException($"{delegateField} field not found"));

//        // Load logger
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?? throw new InvalidOperationException("_logger field not found"));

//        // Load pipe name
//        il.Emit(OpCodes.Ldarg_0);
//        il.Emit(OpCodes.Ldfld, typeof(PipeBlock<C, V>).GetField("_pipeName", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?? throw new InvalidOperationException("_pipeName field not found"));

//        // Load correlation ID  
//        il.Emit(OpCodes.Ldloc, contextLocal);
//        il.Emit(OpCodes.Callvirt, typeof(IContext<V>).GetProperty("CorrelationId")?.GetGetMethod()
//            ?? throw new InvalidOperationException("CorrelationId property not found"));

//        // Load null exception
//        il.Emit(OpCodes.Ldnull);

//        // Invoke delegate
//        il.Emit(OpCodes.Callvirt, typeof(Action<ILogger, string, Guid, Exception?>).GetMethod("Invoke")
//            ?? throw new InvalidOperationException("Action.Invoke method not found"));
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static C RunAsyncBlockSync(IAsyncBlock<C, V> asyncBlock, C context)
//        => ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(context));

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static C ExecuteValueTaskSynchronously(ValueTask<C> task)
//        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
//}