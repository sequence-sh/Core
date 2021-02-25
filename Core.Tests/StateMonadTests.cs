using System;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class StateMonadTests
{
    public interface IDoubleDisposable : IStateDisposable, IDisposable { }

    [Fact]
    public void StateMonadShouldDisposeVariablesThatItRemoves()
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var sd = repo.Create<IDoubleDisposable>();

        var monad = CreateMonad(repo);

        sd.Setup(x => x.Dispose(monad));
        sd.Setup(x => x.Dispose());

        monad.SetVariable(new VariableName("V"), sd.Object, false, null);

        monad.RemoveVariable(new VariableName("V"), true, null);

        repo.VerifyAll();
    }

    [Fact]
    public void StateMonadShouldDisposeVariablesWhenItIsDisposed()
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var sd = repo.Create<IDoubleDisposable>();

        var monad = CreateMonad(repo);

        // ReSharper disable once AccessToDisposedClosure
        sd.Setup(x => x.Dispose(monad));
        sd.Setup(x => x.Dispose());

        monad.SetVariable(new VariableName("V"), sd.Object, false, null);

        monad.Dispose();

        repo.VerifyAll();
    }

    [Fact]
    public void ScopedStateMonadShouldDisposeVariablesThatItRemoves()
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var sd = repo.Create<IDoubleDisposable>();

        var monad1 = CreateMonad(repo);

        var scopedMonad = new ScopedStateMonad(
            monad1,
            ImmutableDictionary<VariableName, object>.Empty
        );

        sd.Setup(x => x.Dispose(scopedMonad));
        sd.Setup(x => x.Dispose());

        scopedMonad.SetVariable(new VariableName("V"), sd.Object, false, null);

        scopedMonad.RemoveVariable(new VariableName("V"), true, null);

        repo.VerifyAll();
    }

    [Fact]
    public void ScopedStateMonadShouldDisposeVariablesWhenItIsDisposed()
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var sd = repo.Create<IDoubleDisposable>();

        var monad1 = CreateMonad(repo);

        var scopedMonad = new ScopedStateMonad(
            monad1,
            ImmutableDictionary<VariableName, object>.Empty
        );

        // ReSharper disable once AccessToDisposedClosure
        sd.Setup(x => x.Dispose(scopedMonad));
        sd.Setup(x => x.Dispose());

        scopedMonad.SetVariable(new VariableName("V"), sd.Object, false, null);

        scopedMonad.Dispose();

        repo.VerifyAll();
    }

    private static IStateMonad CreateMonad(MockRepository repo)
    {
        return
            new StateMonad(
                repo.OneOf<ILogger>(),
                new SCLSettings(Entity.Create()),
                StepFactoryStore.Create(),
                repo.OneOf<IExternalContext>(),
                repo.OneOf<object>()
            );
    }
}

}
