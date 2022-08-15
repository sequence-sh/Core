using System.Text;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Joins strings.
/// Supports string interpolation.
/// To use string interpolation, the string must be enclosed in double
/// quotes and prefixed with a dollar-sign ($). Then, anything in curly
/// brackets { and } will be evaluated as SCL.
/// </summary>
[SCLExample("$\"The answer is {6 + (6 ^ 2)}\"",       "The answer is 42")]
[SCLExample("$\"The answer is {}???\"",               "The answer is ???")]
[SCLExample("$\"(a:{1}, b:{}, c:{3}, d:{}, e:{5})\"", "(a:1, b:, c:3, d:, e:5)")]
[AllowConstantFolding]
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
            var o = await step.Run<ISCLObject>(stateMonad, cancellationToken);

            if (o.IsFailure)
                return o.ConvertFailure<StringStream>();

            var s = o.Value.Serialize(SerializeOptions.Primitive);

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
            public string Serialize(
                SerializeOptions options,
                IEnumerable<StepProperty> stepProperties)
            {
                StringBuilder sb = new();

                sb.Append('$');
                sb.Append('"');

                foreach (var step in stepProperties.Cast<StepProperty.StepListProperty>()
                             .Single()
                             .StepList)
                {
                    if (step is SCLConstant<StringStream> sc)
                    {
                        sb.Append(
                            SerializationMethods.Escape(
                                sc.Value.Serialize(options with { QuoteStrings = false })
                            )
                        );
                    }
                    else
                    {
                        sb.Append('{');
                        var ser = step.Serialize(options);
                        sb.Append(ser);
                        sb.Append('}');
                    }
                }

                sb.Append('"');

                return sb.ToString();
            }

            /// <inheritdoc />
            public void Format(
                IEnumerable<StepProperty> stepProperties,
                TextLocation? textLocation,
                IndentationStringBuilder indentationStringBuilder,
                FormattingOptions options,
                Stack<Comment>? remainingComments = null)
            {
                indentationStringBuilder.AppendPrecedingComments(remainingComments, textLocation);
                indentationStringBuilder.Append("$\"");

                foreach (var step in stepProperties.Cast<StepProperty.StepListProperty>()
                             .Single()
                             .StepList)
                {
                    if (step is SCLConstant<StringStream> sc)
                    {
                        var stepString = SerializationMethods.Escape(
                            sc.Value.Serialize(SerializeOptions.Primitive)
                        );

                        indentationStringBuilder.Append(stepString);
                    }
                    else
                    {
                        indentationStringBuilder.Append("{");
                        step.Format(indentationStringBuilder, options, remainingComments);
                        indentationStringBuilder.Append("}");
                    }
                }

                indentationStringBuilder.Append("");
            }
        }
    }
}
