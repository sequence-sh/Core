using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Appends a string to an existing string variable.
    /// </summary>
    public sealed class AppendString : CompoundStep<Unit>
    {
        /// <summary>
        /// The variable to append to.
        /// </summary>
        [VariableName(1)]
        [Required]
        public VariableName Variable { get; set; }


        /// <summary>
        /// The string to append.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<StringStream> String { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var str = await String.Run(stateMonad, cancellationToken)
                .Map(x => x.GetStringAsync());
            if (str.IsFailure)
                return str.ConvertFailure<Unit>();

            var currentValue = stateMonad.GetVariable<StringStream>(Variable).MapError(x=>x.WithLocation(this));
            if (currentValue.IsFailure)
                return currentValue.ConvertFailure<Unit>();

            var newValue = await currentValue.Value.GetStringAsync() + str.Value;

            var r = stateMonad.SetVariable(Variable, new StringStream(newValue));
            if (r.IsFailure)
                return r.ConvertFailure<Unit>();

            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => AppendStringStepFactory.Instance;
    }

    /// <summary>
    /// Appends a string to an existing string variable.
    /// </summary>
    public sealed class AppendStringStepFactory : SimpleStepFactory<AppendString, Unit>
    {
        private AppendStringStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AppendString, Unit> Instance { get; } = new AppendStringStepFactory();

        /// <inheritdoc />
        public override IEnumerable<(VariableName variableName, Maybe<ITypeReference>)> GetTypeReferencesSet(FreezableStepData freezableStepData, TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(nameof(AppendString.Variable), StepType);
            if(vn.IsFailure) yield break;

            yield return (vn.Value, Maybe<ITypeReference>.From(new ActualTypeReference(typeof(StringStream))));
        }
    }
}