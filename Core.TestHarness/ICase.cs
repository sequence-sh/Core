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

    public void AddContextObject(string name, object o) => _contextObjects.Add((name, o));

    public void AddContextMock(string name, Func<MockRepository, Mock> createFunc) =>
        _contextMocks.Add((name, createFunc));

    private readonly List<(string name, object context)> _contextObjects = new();

    private readonly List<(string name, Func<MockRepository, Mock> contextFunc)> _contextMocks =
        new();

    public IExternalContext GetExternalContext(MockRepository mockRepository)
    {
        var externalProcessRunner = GetAndSetupMock<IExternalProcessRunner>(mockRepository);
        var console               = GetAndSetupMock<IConsole>(mockRepository);
        var file                  = GetAndSetupMock<IFile>(mockRepository);
        var directory             = GetAndSetupMock<IDirectory>(mockRepository);
        var compression           = GetAndSetupMock<ICompression>(mockRepository);

        var objects = new List<(string, object)>();

        objects.AddRange(_contextObjects);

        foreach (var (name, contextFunc) in _contextMocks)
        {
            var mock = contextFunc(mockRepository);
            objects.Add((name, mock.Object));
        }

        var externalContext = new ExternalContext(
            new FileSystemAdapter(directory, file, compression),
            externalProcessRunner,
            console,
            objects.ToArray()
        );

        return externalContext;
    }
}

}
