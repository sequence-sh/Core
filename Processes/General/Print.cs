using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class Print<T> : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var r = Value.Run(processState);
            if (r.IsFailure) return r.ConvertFailure<Unit>();

            processState.Logger.LogInformation(r.Value?.ToString());

            return Unit.Default;
        }

        /// <summary>
        /// The Value to Print.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Value { get; set; } = null!;

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => PrintProcessFactory.Instance;
    }

    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class PrintProcessFactory : GenericProcessFactory
    {
        private PrintProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericProcessFactory Instance { get; } = new PrintProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Print<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder { get; } = new ProcessNameBuilderFromTemplate($"Print [{nameof(Print<object>.Value)}]");

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(Print<object>.Value))
                .Bind(x => x.TryGetOutputTypeReference());

    }
}
