﻿using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class CreateDirectoryTests : StepTestBase<CreateDirectory, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Create Directory",
                new CreateDirectory { Path = Constant("MyPath") },
                Unit.Default
            ).WithDirectoryAction(
                x =>
                    x.Setup(h => h.CreateDirectory("MyPath"))
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Create Directory",
                "CreateDirectory Path: 'MyPath'",
                Unit.Default
            ).WithDirectoryAction(
                x =>
                    x.Setup(h => h.CreateDirectory("MyPath"))
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                    "Error returned",
                    new CreateDirectory { Path = Constant("MyPath") },
                    new ErrorBuilder(ErrorCode.Test, "ValueIf Error")
                )
                .WithDirectoryAction(
                    x =>
                        x.Setup(h => h.CreateDirectory("MyPath"))
                            .Throws<Exception>()
                );
        }
    }
}

}
