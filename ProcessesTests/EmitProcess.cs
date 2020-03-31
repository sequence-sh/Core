using System.Collections.Generic;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class EmitProcess : Process
    {
        [UsedImplicitly]
        [YamlMember]
        public string? Term { get; set; }

        [UsedImplicitly]
        [YamlMember]
        public int? Number { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        public override string GetName()
        {
            return "Emit";
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            return Result.Success<ImmutableProcess, ErrorList>(new ImmutableEmitProcess( Term, Number));
        }
    }

    public class ImmutableEmitProcess : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public ImmutableEmitProcess(string? term, int? number)
        {
            _term = term;
            _number = number;
        }

        private readonly string? _term;

        private readonly int? _number;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return ProcessOutput<Unit>.Message(_term + _number);
        }

        /// <inheritdoc />
        public override string Name => "Emit";
    }
}