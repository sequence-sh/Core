using System.Collections.Generic;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Chain;
using Reductech.EDR.Processes.Output;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Tests
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
        public override Result<IImmutableProcess<TFinal>> TryFreeze<TFinal>(IProcessSettings processSettings)
        {
            var iep = new ImmutableEmitProcess(Term, Number);

            return TryConvertFreezeResult<TFinal, Unit>(iep) ;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return new ChainLinkBuilder<TInput, Unit,TFinal, ImmutableEmitProcess, EmitProcess>(this);
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

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}