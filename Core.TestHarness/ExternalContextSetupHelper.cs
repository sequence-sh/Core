using Sequence.Core.ExternalProcesses;

namespace Sequence.Core.TestHarness;

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

    public IExternalContext GetExternalContext(
        MockRepository mockRepository,
        IRestClientFactory restClientFactory)
    {
        var externalProcessRunner = GetAndSetupMock<IExternalProcessRunner>(mockRepository);
        var console               = GetAndSetupMock<IConsole>(mockRepository);

        var objects = new List<(string, object)>();

        objects.AddRange(_contextObjects);

        foreach (var (name, contextFunc) in _contextMocks)
        {
            var mock = contextFunc(mockRepository);
            objects.Add((name, mock.Object));
        }

        var externalContext = new ExternalContext(
            externalProcessRunner,
            restClientFactory,
            console,
            objects.ToArray()
        );

        return externalContext;
    }
}
