using System.Text;
using Reductech.Sequence.Core.ExternalProcesses;

namespace Reductech.Sequence.Core.Tests.Steps;

public partial class RunExternalProcessTests : StepTestBase<RunExternalProcess, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "Do nothing external process",
                        new RunExternalProcess
                        {
                            Arguments =
                                new ArrayNew<StringStream>()
                                {
                                    Elements = new IStep<StringStream>[] { Constant("Foo") }
                                },
                            Path     = new SCLConstant<StringStream>("Process.exe"),
                            Encoding = Constant(EncodingEnum.Ascii)
                        },
                        Unit.Default,
                        "My Message"
                    )
                    .WithExternalProcessAction(
                        x =>
                            x.Setup(
                                    a => a.RunExternalProcess(
                                        "Process.exe",
                                        It.IsAny<IErrorHandler>(),
                                        It.IsAny<IEnumerable<string>>(),
                                        It.IsAny<IReadOnlyDictionary<string, string>>(),
                                        Encoding.ASCII,
                                        It.IsAny<IStateMonad>(),
                                        It.IsAny<IStep>(),
                                        It.IsAny<CancellationToken>()
                                    )
                                )
                                .Callback<string, IErrorHandler, IEnumerable<string>,
                                    IReadOnlyDictionary<string, string>,
                                    Encoding,
                                    IStateMonad,
                                    IStep,
                                    CancellationToken>(
                                    (
                                        _,
                                        _,
                                        _,
                                        _,
                                        _,
                                        stateMonad,
                                        _,
                                        _) => stateMonad.Log(
                                        LogLevel.Information,
                                        "My Message",
                                        null
                                    )
                                )
                                .ReturnsAsync(Unit.Default)
                    )
                ;
        }
    }
}
