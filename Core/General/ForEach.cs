using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
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
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var elements = Array.Run(stateMonad);
            if (elements.IsFailure) return elements.ConvertFailure<Unit>();

            foreach (var element in elements.Value)
            {
                var setResult = stateMonad.SetVariable(VariableName, element);
                if (setResult.IsFailure) return setResult.ConvertFailure<Unit>();

                var r = Action.Run(stateMonad);
                if (r.IsFailure) return r;
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ForeachStepFactory.Instance;
    }

    /// <summary>
    /// Do an action for each member of the list.
    /// </summary>
    public sealed class ForeachStepFactory : GenericStepFactory
    {
        private ForeachStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new ForeachStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ForEach<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData) =>
            freezableStepData.GetArgument(nameof(ForEach<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData) =>
            freezableStepData.GetArgument(nameof(ForEach<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle()
                .Map(Maybe<ITypeReference>.From);

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"Foreach [{nameof(ForEach<object>.VariableName)}] in [{nameof(ForEach<object>.Array)}]; [{nameof(ForEach<object>.Action)}]");
    }
}