using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Write a value to the logs
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Log<T> : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await Value.Run(stateMonad, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        string stringToPrint = r.Value switch
        {
            Entity entity   => entity.Serialize(),
            StringStream ss => await ss.GetStringAsync(),
            DateTime dt     => dt.ToString(Constants.DateTimeFormat),
            double d        => d.ToString(Constants.DoubleFormat),
            _               => r.Value?.ToString()!
        };

        stateMonad.Log(LogLevel.Information, stringToPrint, this);

        return Unit.Default;
    }

    /// <summary>
    /// The Value to Log.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<T> Value { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => LogStepFactory.Instance;

    private sealed class LogStepFactory : GenericStepFactory
    {
        private LogStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new LogStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Log<>);

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Unit.Instance;

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetMemberType(
            TypeReference expectedTypeReference,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver) => freezableStepData
            .TryGetStep(nameof(Log<object>.Value), StepType)
            .Bind(x => x.TryGetOutputTypeReference(TypeReference.Any.Instance, typeResolver))
            .Map(
                x => x == TypeReference.Any.Instance ? new TypeReference.Actual(SCLType.String) : x
            );
    }
}

}
