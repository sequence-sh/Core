﻿using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests;

[UseTestOutputHelper]
public partial class StepHelpersTest
{
    [Fact]
    public async Task RunStepAsyncMustPropagateErrors()
    {
        var sm = new StateMonad(
            NullLogger.Instance,
            StepFactoryStore.Create(),
            null!,
            new Dictionary<string, object>()
        );

        var result = await sm.RunStepsAsync(
            new StepTestBase<DoNothing, Unit>.FailStep<int>() { ErrorMessage = "Test Error" },
            Constant(2),
            Constant(3),
            Constant(4),
            Constant(5),
            Constant(6),
            Constant(7),
            Constant(8),
            CancellationToken.None
        );

        result.ShouldBeFailure();

        result.Error.AsString.Should().Be("Test Error Message: 'Test Error'");
    }

    [Fact]
    public async Task RunStepAsyncMustWorkWithEightArguments()
    {
        var sm = new StateMonad(
            NullLogger.Instance,
            StepFactoryStore.Create(),
            null!,
            new Dictionary<string, object>()
        );

        var result = await
            sm.RunStepsAsync(
                Constant("a").WrapStringStream(),
                Constant("b").WrapStringStream(),
                Array("d", "e", "f").WrapStringStreamArray(),
                Constant(1),
                Constant(2),
                Constant(3),
                Array(4, 5, 6).WrapArray(),
                (null as IStep<int>).WrapNullable(),
                cancellationToken: CancellationToken.None
            );

        result.ShouldBeSuccessful();

        var (a, b, def, one, two, three, fourFiveSix, nullInt) = result.Value;

        a.Should().Be("a");
        b.Should().Be("b");
        def.Should().BeEquivalentTo("d", "e", "f");
        one.Should().Be(1);
        two.Should().Be(2);
        three.Should().Be(3);
        fourFiveSix.Should().BeEquivalentTo(new[] { 4, 5, 6 });
        nullInt.ShouldHaveNoValue();
    }
}
