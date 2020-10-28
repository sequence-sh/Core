using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class Test<T> : CompoundStep<T>
    {
        /// <inheritdoc />
        public override async Task<Result<T, IError>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await Condition.Run(stateMonad, cancellationToken)
                .Bind(r => r ? ThenValue.Run(stateMonad, cancellationToken) : ElseValue.Run(stateMonad, cancellationToken));

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => TestStepFactory.Instance;


        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> ThenValue { get; set; } = null!;

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> ElseValue { get; set; } = null!;
    }

    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class TestStepFactory : GenericStepFactory
    {
        private TestStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new TestStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Test<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetArgument(nameof(Test<object>.ThenValue), TypeName)
                .Compose(() => freezableStepData.GetArgument(nameof(Test<object>.ElseValue), TypeName))
                .MapError(e=>e.WithLocation(this, freezableStepData))
                .Bind(x => x.Item1.TryGetOutputTypeReference(typeResolver)
                    .Compose(() => x.Item2.TryGetOutputTypeReference(typeResolver)))
                .Bind(x => MultipleTypeReference.TryCreate(new[] { x.Item1, x.Item2 }, TypeName)
                    .MapError(e=>e.WithLocation(this, freezableStepData)))
        ;
    }
}