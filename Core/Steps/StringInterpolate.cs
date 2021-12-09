using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Joins strings.
/// Supports string interpolation.
/// To use string interpolation, the string must be enclosed in double
/// quotes and prefixed with a dollar-sign ($). Then, anything in curly
/// brackets { and } will be evaluated as SCL.
/// </summary>
[SCLExample("$\"The answer is {6 + (6 ^ 2)}\"", "The answer is 42")]
public sealed class StringInterpolate : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        foreach (var step in Strings)
        {
            var o = await step.Run<object>(stateMonad, cancellationToken);

            if (o.IsFailure)
                return o.ConvertFailure<StringStream>();

            var s = await SerializationMethods.GetStringAsync(o.Value);

            sb.Append(s);
        }

        StringStream ss = sb.ToString();

        return ss;
    }

    /// <summary>
    /// The strings to join
    /// </summary>
    [StepListProperty(1)]
    [Required]
    public IReadOnlyList<IStep> Strings { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = StringInterpolateStepFactory.Instance;

    private sealed class
        StringInterpolateStepFactory : SimpleStepFactory<StringInterpolate, StringStream>
    {
        private StringInterpolateStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<StringInterpolate, StringStream> Instance { get; } =
            new StringInterpolateStepFactory();

        /// <inheritdoc />
        public override IStepSerializer Serializer =>
            StringInterpolateSerializer.SerializerInstance;

        private class StringInterpolateSerializer : IStepSerializer
        {
            private StringInterpolateSerializer() { }

            public static IStepSerializer SerializerInstance { get; } =
                new StringInterpolateSerializer();

            /// <inheritdoc />
            public string Serialize(IEnumerable<StepProperty> stepProperties)
            {
                StringBuilder sb = new();

                sb.Append('$');
                sb.Append('"');

                foreach (var step in stepProperties.Cast<StepProperty.StepListProperty>()
                             .Single()
                             .StepList)
                {
                    if (step is StringConstant sc)
                    {
                        sb.Append(SerializationMethods.Escape(sc.Value.GetString()));
                    }
                    else
                    {
                        sb.Append('{');
                        var ser = step.Serialize();
                        sb.Append(ser);
                        sb.Append('}');
                    }
                }

                sb.Append('"');

                return sb.ToString();
            }
        }
    }
}
