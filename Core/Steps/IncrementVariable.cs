﻿using System.ComponentModel.DataAnnotations;
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
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariable : CompoundStep<Unit>
    {
        /// <summary>
        /// The variable to increment.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; }

        /// <summary>
        /// The amount to increment by.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("1")]
        public IStep<int> Amount { get; set; } = new Constant<int>(1);

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var variable = stateMonad.GetVariable<int>(Variable).MapError(x=>x.WithLocation(this));
            if (variable.IsFailure) return variable.ConvertFailure<Unit>();
            var amount = await Amount.Run(stateMonad, cancellationToken);
            if (amount.IsFailure) return amount.ConvertFailure<Unit>();

            var r = stateMonad.SetVariable(Variable, variable.Value + amount.Value);

            return r;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => IncrementVariableStepFactory.Instance;
    }

    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariableStepFactory : SimpleStepFactory<IncrementVariable, int>
    {
        private IncrementVariableStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<IncrementVariable, int> Instance { get; } = new IncrementVariableStepFactory();

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}