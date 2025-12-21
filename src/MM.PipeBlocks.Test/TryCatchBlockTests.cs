//namespace MM.PipeBlocks.Test;
//public class TryCatchBlockTests
//{
//    #region TryCatch
//    [Fact]
//    public void TestTryCatch_WithSuccess()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false;

//        var block = builder.TryCatch(
//            new ReturnValue_CodeBlock(),
//            builder.Run((c, v) =>
//            {
//                executedCatch = true;
//            }));

//        var result = block.Execute(context);

//        Assert.False(executedCatch);
//    }

//    [Fact]
//    public async Task TestTryCatch_WithSuccess_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false;

//        var block = builder.TryCatch(
//            new ReturnValue_AsyncCodeBlock(),
//            builder.Run((c, v) =>
//            {
//                executedCatch = true;
//            }));
        
//        var result = await block.ExecuteAsync(context);

//        Assert.False(executedCatch);
//    }

//    [Fact]
//    public void TestCatch_WithException()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false;
//        Guid sniffedId = Guid.NewGuid();

//        try
//        {
//            var block = builder.TryCatch(
//                new Exception_CodeBlock(),
//                new FuncBlock<MyContext, MyValue>((c, v) =>
//                {
//                    executedCatch = true;
//                    sniffedId = v.Identifier;
//                }));

//            var result = block.Execute(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(executedCatch);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public void TestCatch_WithFailure()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false;
//        Guid sniffedId = Guid.NewGuid();

//        var block = builder.TryCatch(
//            new ReturnFailContext_CodeBlock(),
//            new FuncBlock<MyContext, MyValue>((c, v) =>
//            {
//                executedCatch = true;
//                sniffedId = v.Identifier;
//            }));

//        var result = block.Execute(context);

//        Assert.True(executedCatch);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public async Task TestCatch_WithException_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false;
//        Guid sniffedId = Guid.NewGuid();

//        try
//        {
//            var block = builder.TryCatch(
//                new Exception_CodeBlock(),
//                new FuncBlock<MyContext, MyValue>((c, v) =>
//                {
//                    executedCatch = true;
//                    sniffedId = v.Identifier;
//                }));

//            var result = await block.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(executedCatch);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public async Task TestCatch_WithFailure_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false;
//        Guid sniffedId = Guid.NewGuid();

//        var block = builder.TryCatch(
//            new ReturnFailContext_CodeBlock(),
//            new FuncBlock<MyContext, MyValue>((c, v) =>
//            {
//                executedCatch = true;
//                sniffedId = v.Identifier;
//            }));

//        var result = await block.ExecuteAsync(context);

//        Assert.True(executedCatch);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public void TestTryCatchGeneric()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var block = builder.TryCatch<Exception_CodeBlock, SniffCatchBlock>();

//            var result = block.Execute(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedCatch);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }

//    [Fact]
//    public async Task TestTryCatch_Async()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var pipe = builder.CreatePipe("test trycatch async")
//                        .Then(b => b.TryCatch(
//                            new Exception_AsyncCodeBlock(),
//                            b.Run((c, v) =>
//                            {
//                                c.ExecutedCatch = true;
//                                c.SniffedId = v.Identifier;
//                                return ValueTask.CompletedTask;
//                            })));

//            var result = await pipe.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedCatch);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }

//    [Fact]
//    public async Task TestTryCatchGeneric_Async()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var block = builder.TryCatch<Exception_AsyncCodeBlock, SniffCatchAsyncBlock>();

//            var result = await block.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedCatch);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }
//    #endregion

//    #region TryFinally
//    [Fact]
//    public void TestFinally_WithException()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        try
//        {
//            var block = builder.TryFinally(
//                new Exception_CodeBlock(),
//                new FuncBlock<MyContext, MyValue>((c, v) =>
//                {
//                    executedFinally = true;
//                    sniffedId = v.Identifier;
//                }));

//            var result = block.Execute(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public void TestFinally_WithFailure()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        var block = builder.TryFinally(
//            new ReturnFailContext_CodeBlock(),
//            new FuncBlock<MyContext, MyValue>((c, v) =>
//            {
//                executedFinally = true;
//                sniffedId = v.Identifier;
//            }));

//        var result = block.Execute(context);

//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public async Task TestFinally_WithException_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        try
//        {
//            var block = builder.TryFinally(
//                new Exception_CodeBlock(),
//                new FuncBlock<MyContext, MyValue>((c, v) =>
//                {
//                    executedFinally = true;
//                    sniffedId = v.Identifier;
//                }));

//            var result = await block.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public async Task TestFinally_WithFailure_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        var block = builder.TryFinally(
//            new ReturnFailContext_CodeBlock(),
//            new FuncBlock<MyContext, MyValue>((c, v) =>
//            {
//                executedFinally = true;
//                sniffedId = v.Identifier;
//            }));

//        var result = await block.ExecuteAsync(context);

//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public void TestTryFinallyGeneric()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var block = builder.TryFinally<Exception_CodeBlock, SniffFinallyBlock>();

//            var result = block.Execute(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedFinally);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }

//    [Fact]
//    public async Task TestTryFinally_Async()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var pipe = builder.TryFinally(
//                            new Exception_AsyncCodeBlock(),
//                            new AsyncFuncBlock<MyContext, MyValue>((c, v) =>
//                            {
//                                c.ExecutedFinally = true;
//                                c.SniffedId = v.Identifier;
//                                return ValueTask.CompletedTask;
//                            }));

//            var result = await pipe.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedFinally);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }

//    [Fact]
//    public async Task TestTryFinallyGeneric_Async()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var block = builder.TryFinally<Exception_AsyncCodeBlock, SniffFinallyAsyncBlock>();

//            var result = await block.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedFinally);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }
//    #endregion

//    #region TryCatchFinally
//    [Fact]
//    public void TestCatchFinally_WithException()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false,
//             executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        try
//        {
//            var block = builder.TryCatchFinally(
//                new Exception_CodeBlock(),
//                new FuncBlock<MyContext, MyValue>((c, v) =>
//                {
//                    executedCatch = true;
//                    sniffedId = v.Identifier;
//                }),
//                builder.Run((c, v) =>
//                {
//                    executedFinally = true;
//                    sniffedId = v.Identifier;
//                }));

//            var result = block.Execute(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(executedCatch);
//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public void TestCatchFinally_WithFailure()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false,
//             executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        var block = builder.TryCatchFinally(
//            new ReturnFailContext_CodeBlock(),
//            new FuncBlock<MyContext, MyValue>((c, v) =>
//            {
//                executedCatch = true;
//                sniffedId = v.Identifier;
//            }),
//            builder.Run((c, v) =>
//            {
//                executedFinally = true;
//                sniffedId = v.Identifier;
//            }));

//        var result = block.Execute(context);

//        Assert.True(executedCatch);
//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public async Task TestCatchFinally_WithException_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false,
//             executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        try
//        {
//            var block = builder.TryCatchFinally(
//                new Exception_CodeBlock(),
//                new FuncBlock<MyContext, MyValue>((c, v) =>
//                {
//                    executedCatch = true;
//                    sniffedId = v.Identifier;
//                }),
//                builder.Run((c, v) =>
//                {
//                    executedFinally = true;
//                    sniffedId = v.Identifier;
//                }));

//            var result = await block.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(executedCatch);
//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public async Task TestCatchFinally_WithFailure_Async()
//    {
//        var builder = new BlockBuilder<MyContext, MyValue>();
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        bool executedCatch = false,
//             executedFinally = false;
//        Guid sniffedId = Guid.NewGuid();

//        var block = builder.TryCatchFinally(
//            new ReturnFailContext_CodeBlock(),
//            new FuncBlock<MyContext, MyValue>((c, v) =>
//            {
//                executedCatch = true;
//                sniffedId = v.Identifier;
//            }),
//            builder.Run((c, v) =>
//            {
//                executedFinally = true;
//                sniffedId = v.Identifier;
//            }));

//        var result = await block.ExecuteAsync(context);

//        Assert.True(executedCatch);
//        Assert.True(executedFinally);
//        Assert.Equal(initialValue.Identifier, sniffedId);
//    }

//    [Fact]
//    public void TestTryCatchFinallyGeneric()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var block = builder.TryCatchFinally<Exception_CodeBlock, SniffCatchBlock, SniffFinallyBlock>();

//            var result = block.Execute(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedCatch);
//        Assert.True(context.ExecutedFinally);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }

//    [Fact]
//    public async Task TestTryCatchFinally_Async()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var pipe = builder.TryCatchFinally(
//                            new Exception_AsyncCodeBlock(),
//                            builder.Run((c, v) =>
//                            {
//                                c.ExecutedCatch = true;
//                                c.SniffedId = v.Identifier;
//                                return ValueTask.CompletedTask;
//                            }),
//                            builder.Run((c, v) =>
//                            {
//                                c.ExecutedFinally = true;
//                                c.SniffedId = v.Identifier;
//                                return ValueTask.CompletedTask;
//                            }));

//            var result = await pipe.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedCatch);
//        Assert.True(context.ExecutedFinally);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }

//    [Fact]
//    public async Task TestTryCatchFinallyGeneric_Async()
//    {
//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        try
//        {
//            var builder = new BlockBuilder<MyContext, MyValue>();
//            var block = builder.TryCatchFinally<Exception_AsyncCodeBlock, SniffCatchAsyncBlock, SniffFinallyAsyncBlock>();

//            var result = await block.ExecuteAsync(context);
//        }
//        catch (Exception ex)
//        {
//            Assert.Equal("Intentional", ex.Message);
//        }

//        Assert.True(context.ExecutedCatch);
//        Assert.True(context.ExecutedFinally);
//        Assert.Equal(initialValue.Identifier, context.SniffedId);
//    }
//    #endregion
//}