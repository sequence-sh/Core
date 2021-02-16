﻿using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

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
                            Path     = new StringConstant("Process.exe"),
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
                                        It.IsAny<ILogger>(),
                                        It.IsAny<IErrorHandler>(),
                                        It.IsAny<IEnumerable<string>>(),
                                        It.IsAny<IReadOnlyDictionary<string, string>>(),
                                        Encoding.ASCII,
                                        It.IsAny<CancellationToken>()
                                    )
                                )
                                .Callback<string, ILogger, IErrorHandler, IEnumerable<string>,
                                    IReadOnlyDictionary<string, string>,
                                    Encoding, CancellationToken>(
                                    (
                                        _,
                                        b,
                                        _,
                                        _,
                                        _,
                                        _,
                                        _) => b.LogInformation("My Message")
                                )
                                .ReturnsAsync(Unit.Default)
                    )
                ;
        }
    }
}

}
