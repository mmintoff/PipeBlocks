using MM.PipeBlocks.Abstractions;
using Polly;

namespace MM.PipeBlocks.Test;

public class TryCatchBlockTests
{
    #region TryCatch
    [Fact]
    public void TestTryCatch_WithSuccess()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        bool executedCatch = false;

        var block = builder.TryCatch(
            new ReturnValue_CodeBlock(),
            builder.Run(v =>
            {
                executedCatch = true;
            }));

        _ = block.Execute(initialValue);

        Assert.False(executedCatch);
    }

    [Fact]
    public async Task TestTryCatch_WithSuccess_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        bool executedCatch = false;

        var block = builder.TryCatch(
            new ReturnValue_AsyncCodeBlock(),
            builder.Run(v =>
            {
                executedCatch = true;
            }));

        _ = await block.ExecuteAsync(initialValue);

        Assert.False(executedCatch);
    }

    [Fact]
    public void TestCatch_WithException()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        bool executedCatch = false;
        Guid sniffedId = Guid.NewGuid();

        try
        {
            var block = builder.TryCatch(
                new Exception_CodeBlock(),
                new FuncBlock<MyValue>(v =>
                {
                    executedCatch = true;
                    sniffedId = v.Value.Identifier;
                }));

            _ = block.Execute(initialValue);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(executedCatch);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public void TestCatch_WithFailure()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        bool executedCatch = false;
        Guid sniffedId = Guid.NewGuid();

        var block = builder.TryCatch(
            new ReturnFailContext_CodeBlock(),
            new FuncBlock<MyValue>(v =>
            {
                executedCatch = true;
                sniffedId = v.Value.Identifier;
            }));

        _ = block.Execute(initialValue);

        Assert.True(executedCatch);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public async Task TestCatch_WithException_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        bool executedCatch = false;
        Guid sniffedId = Guid.NewGuid();

        try
        {
            var block = builder.TryCatch(
                new Exception_CodeBlock(),
                new FuncBlock<MyValue>(v =>
                {
                    executedCatch = true;
                    sniffedId = v.Value.Identifier;
                }));

            _ = await block.ExecuteAsync(initialValue);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(executedCatch);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public async Task TestCatch_WithFailure_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };

        bool executedCatch = false;
        Guid sniffedId = Guid.NewGuid();

        var block = builder.TryCatch(
            new ReturnFailContext_CodeBlock(),
            new FuncBlock<MyValue>(v =>
            {
                executedCatch = true;
                sniffedId = v.Value.Identifier;
            }));

        _ = await block.ExecuteAsync(initialValue);

        Assert.True(executedCatch);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public void TestTryCatchGeneric()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var block = builder.TryCatch<Exception_CodeBlock, SniffCatchBlock>();

            _ = block.Execute(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.TryGet<bool>("ExecutedCatch", out var ctxExecuted) && ctxExecuted);
        Assert.Equal(initialValue.Identifier, value.Context.TryGet<Guid>("SniffedId", out var ctxSniffedId) ? ctxSniffedId : Guid.Empty);
    }

    [Fact]
    public async Task TestTryCatch_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var pipe = builder.CreatePipe("test trycatch async")
                        .Then(b => b.TryCatch(
                            new Exception_AsyncCodeBlock(),
                            b.Run(async v =>
                            {
                                v.Context.Set("ExecutedCatch", true);
                                v.Context.Set("SniffedId", v.Value.Identifier);
                                await ValueTask.CompletedTask;
                            })));

            _ = await pipe.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.TryGet<bool>("ExecutedCatch", out var ctxExecuted) && ctxExecuted);
        Assert.Equal(initialValue.Identifier, value.Context.TryGet<Guid>("SniffedId", out var ctxSniffedId) ? ctxSniffedId : Guid.Empty);
    }

    [Fact]
    public async Task TestTryCatchGeneric_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var block = builder.TryCatch<Exception_AsyncCodeBlock, SniffCatchAsyncBlock>();

            _ = await block.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.TryGet<bool>("ExecutedCatch", out var ctxExecuted) && ctxExecuted);
        Assert.Equal(initialValue.Identifier, value.Context.TryGet<Guid>("SniffedId", out var ctxSniffedId) ? ctxSniffedId : Guid.Empty);
    }
    #endregion

    #region TryFinally
    [Fact]
    public void TestFinally_WithException()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        try
        {
            var block = builder.TryFinally(
                new Exception_CodeBlock(),
                new FuncBlock<MyValue>(v =>
                {
                    executedFinally = true;
                    sniffedId = v.Value.Identifier;
                }));

            var result = block.Execute(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public void TestFinally_WithFailure()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        var block = builder.TryFinally(
            new ReturnFailContext_CodeBlock(),
            new FuncBlock<MyValue>(v =>
            {
                executedFinally = true;
                sniffedId = v.Value.Identifier;
            }));

        var result = block.Execute(value);

        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public async Task TestFinally_WithException_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        try
        {
            var block = builder.TryFinally(
                new Exception_CodeBlock(),
                new FuncBlock<MyValue>(v =>
                {
                    executedFinally = true;
                    sniffedId = v.Value.Identifier;
                }));

            var result = await block.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public async Task TestFinally_WithFailure_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        var block = builder.TryFinally(
            new ReturnFailContext_CodeBlock(),
            new FuncBlock<MyValue>(v =>
            {
                executedFinally = true;
                sniffedId = v.Value.Identifier;
            }));

        var result = await block.ExecuteAsync(value);

        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public void TestTryFinallyGeneric()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var block = builder.TryFinally<Exception_CodeBlock, SniffFinallyBlock>();

            var result = block.Execute(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.TryGet<bool>("ExecutedFinally", out var ctxExecuted) && ctxExecuted);
        Assert.Equal(initialValue.Identifier, value.Context.TryGet<Guid>("SniffedId", out var ctxSniffedId) ? ctxSniffedId : Guid.Empty);
    }

    [Fact]
    public async Task TestTryFinally_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var pipe = builder.TryFinally(
                            new Exception_AsyncCodeBlock(),
                            new AsyncFuncBlock<MyValue>(v =>
                            {
                                v.Context.Set("ExecutedFinally", true);
                                v.Context.Set("SniffedId", v.Value.Identifier);
                                return ValueTask.CompletedTask;
                            }));

            _ = await pipe.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.TryGet<bool>("ExecutedFinally", out var ctxExecuted) && ctxExecuted);
        Assert.Equal(initialValue.Identifier, value.Context.TryGet<Guid>("SniffedId", out var ctxSniffedId) ? ctxSniffedId : Guid.Empty);
    }

    [Fact]
    public async Task TestTryFinallyGeneric_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var block = builder.TryFinally<Exception_AsyncCodeBlock, SniffFinallyAsyncBlock>();

            _ = await block.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.TryGet<bool>("ExecutedFinally", out var ctxExecuted) && ctxExecuted);
        Assert.Equal(initialValue.Identifier, value.Context.TryGet<Guid>("SniffedId", out var ctxSniffedId) ? ctxSniffedId : Guid.Empty);
    }
    #endregion

    #region TryCatchFinally
    [Fact]
    public void TestCatchFinally_WithException()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedCatch = false,
             executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        try
        {
            var block = builder.TryCatchFinally(
                new Exception_CodeBlock(),
                new FuncBlock<MyValue>(v =>
                {
                    executedCatch = true;
                    sniffedId = v.Value.Identifier;
                }),
                builder.Run(v =>
                {
                    executedFinally = true;
                    sniffedId = v.Value.Identifier;
                }));

            _ = block.Execute(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(executedCatch);
        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public void TestCatchFinally_WithFailure()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedCatch = false,
             executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        var block = builder.TryCatchFinally(
            new ReturnFailContext_CodeBlock(),
            new FuncBlock<MyValue>(v =>
            {
                executedCatch = true;
                sniffedId = v.Value.Identifier;
            }),
            builder.Run(v =>
            {
                executedFinally = true;
                sniffedId = v.Value.Identifier;
            }));

        _ = block.Execute(value);

        Assert.True(executedCatch);
        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public async Task TestCatchFinally_WithException_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedCatch = false,
             executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        try
        {
            var block = builder.TryCatchFinally(
                new Exception_CodeBlock(),
                new FuncBlock<MyValue>(v =>
                {
                    executedCatch = true;
                    sniffedId = v.Value.Identifier;
                }),
                builder.Run(v =>
                {
                    executedFinally = true;
                    sniffedId = v.Value.Identifier;
                }));

            _ = await block.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(executedCatch);
        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public async Task TestCatchFinally_WithFailure_Async()
    {
        var builder = new BlockBuilder<MyValue>();
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        bool executedCatch = false,
             executedFinally = false;
        Guid sniffedId = Guid.NewGuid();

        var block = builder.TryCatchFinally(
            new ReturnFailContext_CodeBlock(),
            new FuncBlock<MyValue>(v =>
            {
                executedCatch = true;
                sniffedId = v.Value.Identifier;
            }),
            builder.Run(v =>
            {
                executedFinally = true;
                sniffedId = v.Value.Identifier;
            }));

        _ = await block.ExecuteAsync(value);

        Assert.True(executedCatch);
        Assert.True(executedFinally);
        Assert.Equal(initialValue.Identifier, sniffedId);
    }

    [Fact]
    public void TestTryCatchFinallyGeneric()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var block = builder.TryCatchFinally<Exception_CodeBlock, SniffCatchBlock, SniffFinallyBlock>();

            var result = block.Execute(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.Get<bool>("ExecutedCatch"));
        Assert.True(value.Context.Get<bool>("ExecutedFinally"));
        Assert.Equal(initialValue.Identifier, value.Context.Get<Guid>("SniffedId"));
    }

    [Fact]
    public async Task TestTryCatchFinally_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var pipe = builder.TryCatchFinally(
                            new Exception_AsyncCodeBlock(),
                            builder.Run(v =>
                            {
                                v.Context.Set("ExecutedCatch", true);
                                v.Context.Set("SniffedId", v.Value.Identifier);
                                return ValueTask.CompletedTask;
                            }),
                            builder.Run(v =>
                            {
                                v.Context.Set("ExecutedFinally", true);
                                v.Context.Set("SniffedId", v.Value.Identifier);
                                return ValueTask.CompletedTask;
                            }));

            var result = await pipe.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.Get<bool>("ExecutedCatch"));
        Assert.True(value.Context.Get<bool>("ExecutedFinally"));
        Assert.Equal(initialValue.Identifier, value.Context.Get<Guid>("SniffedId"));
    }

    [Fact]
    public async Task TestTryCatchFinallyGeneric_Async()
    {
        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
        var value = new Parameter<MyValue>(initialValue);

        try
        {
            var builder = new BlockBuilder<MyValue>();
            var block = builder.TryCatchFinally<Exception_AsyncCodeBlock, SniffCatchAsyncBlock, SniffFinallyAsyncBlock>();

            var result = await block.ExecuteAsync(value);
        }
        catch (Exception ex)
        {
            Assert.Equal("Intentional", ex.Message);
        }

        Assert.True(value.Context.Get<bool>("ExecutedCatch"));
        Assert.True(value.Context.Get<bool>("ExecutedFinally"));
        Assert.Equal(initialValue.Identifier, value.Context.Get<Guid>("SniffedId"));
    }
    #endregion
}