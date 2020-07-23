﻿using System.Collections.Generic;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Tests
{
    public class ImmutableReturnIntProcess : ImmutableProcess<int>
    {
        public ImmutableReturnIntProcess(int value)
        {
            Value = value;
        }

        public int Value { get; }

        /// <inheritdoc />
#pragma warning disable 1998
        public override async IAsyncEnumerable<IProcessOutput<int>> Execute()
#pragma warning restore 1998
        {
            yield return ProcessOutput<int>.Success(Value);
        }

        /// <inheritdoc />
        public override string Name => "Return " + Value;

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}