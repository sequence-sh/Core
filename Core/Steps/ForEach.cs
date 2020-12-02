using System;
using System.Collections.Generic;
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
    /// Do an action for each member of the list.
    /// </summary>
    public sealed class ForEach<T> : CompoundStep<Unit>
    {
        /// <summary>
        /// The action to perform repeatedly.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;


        /// <summary>
        /// The name of the variable to loop over.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; } //TODO use x

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var elements = await Array.Run(stateMonad, cancellationToken);
            if (elements.IsFailure) return elements.ConvertFailure<Unit>();

            foreach (var element in elements.Value)
            {
                var setResult = stateMonad.SetVariable(Variable, element);
                if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

                var r = await Action.Run(stateMonad, cancellationToken);
                if (r.IsFailure) return r;
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ForEachStepFactory.Instance;
    }

    /// <summary>
    /// Do an action for each member of the list.
    /// </summary>
    public sealed class ForEachStepFactory : GenericStepFactory
    {
        private ForEachStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new ForEachStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ForEach<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetStep(nameof(ForEach<object>.Array), TypeName)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x=>x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e=>e.WithLocation(this, freezableStepData))
                )
                .Map(x=> x as ITypeReference);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver, StepFactoryStore stepFactoryStore) =>
            GetMemberType(freezableStepData, typeResolver)
                .Map(Maybe<ITypeReference>.From);

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Foreach [{nameof(ForEach<object>.Variable)}] in [{nameof(ForEach<object>.Array)}]; [{nameof(ForEach<object>.Action)}]");
    }
}