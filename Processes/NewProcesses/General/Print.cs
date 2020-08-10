using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Print<T> : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            var r = Value.Run(processState);
            if (r.IsFailure) return r.ConvertFailure<Unit>();

            processState.Logger.LogInformation(r.Value.ToString());

            return Result.Success(Unit.Default);
        }

        /// <summary>
        /// The Value to Print.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Value { get; set; } = null!;

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => PrintProcessFactory.Instance;
    }

    public sealed class PrintProcessFactory : RunnableProcessFactory
    {
        private PrintProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new PrintProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override string TypeName => FormatTypeName(typeof(Print<>));

        /// <inheritdoc />
        public override string GetProcessName(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var value = processArguments.TryFind(nameof(Print<object>.Value))
                .Unwrap(NameHelper.MissingProcess.Instance);

            return NameHelper.GetPrintName(value);
        }

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            processArguments.TryFindOrFail(nameof(Print<object>.Value), "Could not get Print value.")
                .Bind(x => x.TryGetOutputTypeReference())
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(typeof(Print<>), x));
    }
}
