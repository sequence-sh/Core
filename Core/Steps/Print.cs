using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class Print<T> : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var r = await Value.Run(stateMonad, cancellationToken);
            if (r.IsFailure) return r.ConvertFailure<Unit>();


            string stringToPrint;

            if (r.Value is Entity entity)
                stringToPrint = entity.Serialize();
            else
                stringToPrint = r.Value?.ToString()!;

            stateMonad.Logger.LogInformation(stringToPrint);

            return Unit.Default;
        }

        /// <summary>
        /// The Value to Print.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<T> Value { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => PrintStepFactory.Instance;
    }

    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class PrintStepFactory : GenericStepFactory
    {
        private PrintStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new PrintStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Print<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.TryGetStep(nameof(Print<object>.Value), StepType)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver));
    }
}
