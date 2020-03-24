using System.Collections.Generic;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
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

        public override string GetName()
        {
            return ("Emit " + Term + Number).Trim();
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            return Result.Success<ImmutableProcess, ErrorList>(new FrozenEmitProcess(GetName(), Term, Number));
        }
    }

    public class FrozenEmitProcess : ImmutableProcess
    {
        /// <inheritdoc />
        public FrozenEmitProcess(string name, string? term, int? number) : base(name)
        {
            _term = term;
            _number = number;
        }

        private readonly string? _term;

        private readonly int? _number;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return Result.Success(_term + _number);
        }
    }
}