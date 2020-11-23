﻿using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class RunExternalProcessTests : StepTestBase<RunExternalProcess, Unit>
    {
        /// <inheritdoc />
        public RunExternalProcessTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield return new StepCase("Do nothing external process",

                        new RunExternalProcess
                        {
                            Arguments = new Constant<List<string>>(new List<string>(){"Foo"}),
                            ProcessPath = new Constant<string>("Process.exe"),
                            Encoding = Constant(EncodingEnum.Ascii)
                        },
                        Unit.Default, "My Message"
                    )
                    .WithExternalProcessAction(x=>
                        x.Setup(a => a.RunExternalProcess("Process.exe",
                                It.IsAny<ILogger>(),
                                It.IsAny<IErrorHandler>(),
                                It.IsAny<IEnumerable<string>>(),
                                Encoding.ASCII,
                                It.IsAny<CancellationToken>()
                                ))
                            .Callback<string, ILogger, IErrorHandler, IEnumerable<string>, Encoding, CancellationToken>((a,b,c,d, e, ct)=> b.LogInformation("My Message"))
                            .ReturnsAsync(Unit.Default)
                    )

                ;}
        }

    }
}