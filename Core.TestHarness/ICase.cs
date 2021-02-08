using System;
using System.Collections.Generic;
using System.Linq;
using AutoTheory;
using CSharpFunctionalExtensions;
using Moq;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Thinktecture;
using Thinktecture.IO;

namespace Reductech.EDR.Core.TestHarness
{

/// <summary>
/// A case that executes a step.
/// </summary>
public interface ICaseThatExecutes : IAsyncTestInstance, ICaseWithSetup
{
    Dictionary<VariableName, object> ExpectedFinalState { get; }

    public bool IgnoreFinalState { get; set; }

    Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }

    SCLSettings Settings { get; set; }
}

public interface ICaseWithSetup
{
    ExternalContextSetupHelper ExternalContextSetupHelper { get; }
}

public sealed class ExternalContextSetupHelper
{
    public void AddSetupAction<T>(Action<Mock<T>, MockRepository> action)
        where T : class => _setupActions.Add(action);

    private readonly List<object> _setupActions = new();

    public T GetAndSetupMock<T>(MockRepository mockRepository)
        where T : class
    {
        var mock = mockRepository.Create<T>();

        foreach (var setupAction in _setupActions.OfType<Action<Mock<T>, MockRepository>>())
        {
            setupAction(mock, mockRepository);
        }

        return mock.Object;
    }

    public void AddContextObject(object o) => _contextObjects.Add(o);

    public void AddContextMock(Func<MockRepository, Mock> createFunc) =>
        _contextMocks.Add(createFunc);

    private readonly List<object> _contextObjects = new();
    private readonly List<Func<MockRepository, Mock>> _contextMocks = new();

    public IExternalContext GetExternalContext(MockRepository mockRepository)
    {
        var externalProcessRunner = GetAndSetupMock<IExternalProcessRunner>(mockRepository);
        var console               = GetAndSetupMock<IConsole>(mockRepository);
        var file                  = GetAndSetupMock<IFile>(mockRepository);
        var directory             = GetAndSetupMock<IDirectory>(mockRepository);
        var compression           = GetAndSetupMock<ICompression>(mockRepository);

        var objects = new List<object>();

        objects.AddRange(_contextObjects);

        foreach (var contextMock in _contextMocks)
        {
            var mock = contextMock(mockRepository);
            objects.Add(mock.Object);
        }

        var externalContext = new ExternalContext(
            new FileSystemAdapter(directory, file, compression),
            externalProcessRunner,
            console
        );

        return externalContext;
    }
}

}
