//using MM.PipeBlocks.Abstractions;

//namespace MM.PipeBlocks.Test.BlockBuilderTests;
//public class ReturnTests
//{
//    private readonly BlockBuilder<MyContext, MyValue> _blockBuilder = new();

//    private void AssertIsReturnBlock(IBlock<MyContext, MyValue> block)
//    {
//        Assert.IsType<ReturnBlock<MyContext, MyValue>>(block);
//    }

//    private void AssertIsBranchBlock(IBlock<MyContext, MyValue> block)
//    {
//        Assert.IsType<BranchBlock<MyContext, MyValue>>(block);
//    }

//    [Fact]
//    public void Noop()
//    {
//        var block = _blockBuilder.Noop();
        
//        Assert.IsType<NoopBlock<MyContext, MyValue>>(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
//            s => Assert.Equal(context.CorrelationId, result.CorrelationId));
//    }

//    [Fact]
//    public void Return_C()
//    {
//        var block = _blockBuilder.Return();

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(initialValue);

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Fail(f.FailureReason ?? "Empty FailureReason"),
//            s => Assert.Equal(context.CorrelationId, result.CorrelationId));
//    }

//    [Theory]
//    [InlineData(true, 0)]
//    [InlineData(false, 1)]
//    public void Return_C_Action(bool isFinished, int expected)
//    {
//        var block = _blockBuilder.Return(c =>
//        {
//            c.Counter++;
//        });

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s =>
//            {
//                Assert.Equal(expected, context.Counter);
//                Assert.True(context.IsFinished);
//            });
//    }

//    [Theory]
//    [InlineData(true, 0)]
//    [InlineData(false, 1)]
//    public void Return_C_Func_C(bool isFinished, int expected)
//    {
//        var block = _blockBuilder.Return(c =>
//        {
//            c.Counter++;
//            return c;
//        });

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s =>
//            {
//                Assert.Equal(expected, context.Counter);
//                Assert.True(context.IsFinished);
//            });
//    }

//    [Theory]
//    [InlineData(true, 0)]
//    [InlineData(false, 1)]
//    public void Return_CV_Func_C(bool isFinished, int expected)
//    {
//        var block = _blockBuilder.Return((c, v) =>
//        {
//            c.Counter++;
//            return c;
//        });

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s =>
//            {
//                Assert.Equal(expected, context.Counter);
//                Assert.True(context.IsFinished);
//            });
//    }

//    [Theory]
//    [InlineData(true, 0)]
//    [InlineData(false, 1)]
//    public void Return_C_Func_ValueTask(bool isFinished, int expected)
//    {
//        var block = _blockBuilder.Return(c =>
//        {
//            c.Counter++;
//            return ValueTask.CompletedTask;
//        });

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s =>
//            {
//                Assert.Equal(expected, context.Counter);
//                Assert.True(context.IsFinished);
//            });
//    }

//    [Theory]
//    [InlineData(true, 0)]
//    [InlineData(false, 1)]
//    public void Return_C_Func_ValueTask_C(bool isFinished, int expected)
//    {
//        var block = _blockBuilder.Return(c =>
//        {
//            c.Counter++;
//            return ValueTask.FromResult(c);
//        });

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s =>
//            {
//                Assert.Equal(expected, context.Counter);
//                Assert.True(context.IsFinished);
//            });
//    }

//    [Theory]
//    [InlineData(true, 0)]
//    [InlineData(false, 1)]
//    public void Return_CV_Func_ValueTask_C(bool isFinished, int expected)
//    {
//        var block = _blockBuilder.Return((c, v) =>
//        {
//            c.Counter++;
//            return ValueTask.FromResult(c);
//        });

//        AssertIsReturnBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s =>
//            {
//                Assert.Equal(expected, context.Counter);
//                Assert.True(context.IsFinished);
//            });
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 0)] // Will return, therefore will not increment
//    [InlineData(false, false, 1)] // Will not return, therefore will increment
//    public void ReturnIf_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition);

//        AssertIsBranchBlock(block);

//        var pipe = _blockBuilder.CreatePipe("ReturnIf")
//            .Then(block)
//            .Then(b => b.Run(c => c.Counter++))
//            ;

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = pipe.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_C_Action(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, c => c.Counter++);

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_CV_Action(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, (c, v) => c.Counter++);

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_C_Func_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, c =>
//        {
//            c.Counter++;
//            return c;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_CV_Func_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, (c, v) =>
//        {
//            c.Counter++;
//            return c;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_C_Func_ValueTask(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, c =>
//        {
//            c.Counter++;
//            return ValueTask.CompletedTask;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_CV_Func_ValueTask(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, (c, v) =>
//        {
//            c.Counter++;
//            return ValueTask.CompletedTask;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_C_Func_ValueTask_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, c =>
//        {
//            c.Counter++;
//            return ValueTask.FromResult(c);
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_CV_Func_ValueTask_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => condition, (c, v) =>
//        {
//            c.Counter++;
//            return ValueTask.FromResult(c);
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_C_Action(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), c =>
//        {
//            c.Counter++;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_CV_Action(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), (c, v) =>
//        {
//            c.Counter++;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_C_Func_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), c =>
//        {
//            c.Counter++;
//            return c;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_CV_Func_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), (c, v) =>
//        {
//            c.Counter++;
//            return c;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_C_Func_ValueTask(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), c =>
//        {
//            c.Counter++;
//            return ValueTask.CompletedTask;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_CV_Func_ValueTask(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), (c, v) =>
//        {
//            c.Counter++;
//            return ValueTask.CompletedTask;
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_C_Func_ValueTask_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), c =>
//        {
//            c.Counter++;
//            return ValueTask.FromResult(c);
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }

//    [Theory]
//    [InlineData(true, true, 0)] // Auto-fail
//    [InlineData(true, false, 0)] // Auto-fail
//    [InlineData(false, true, 1)] // Will return, therefore will increment
//    [InlineData(false, false, 0)] // Will not return, therefore will not increment
//    public void ReturnIf_Async_CV_Func_ValueTask_C(bool isFinished, bool condition, int expected)
//    {
//        var block = _blockBuilder.ReturnIf(c => ValueTask.FromResult(condition), (c, v) =>
//        {
//            c.Counter++;
//            return ValueTask.FromResult(c);
//        });

//        AssertIsBranchBlock(block);

//        var initialValue = new MyValue { Identifier = Guid.NewGuid() };
//        var context = new MyContext(new MyValue())
//        {
//            Value = isFinished
//                ? new Either<IFailureState<MyValue>, MyValue>(new DefaultFailureState<MyValue>(initialValue))
//                : new Either<IFailureState<MyValue>, MyValue>(initialValue),
//            IsFinished = isFinished
//        };

//        var result = block.Execute(context);
//        result.Value.Match(
//            f => Assert.Equal(expected, context.Counter),
//            s => Assert.Equal(expected, context.Counter));
//    }
//}