namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A property of a step.
/// Includes both the parameter and the argument
/// </summary>
public abstract record StepProperty
(
    StepParameter StepParameter,
    int Index, //Note this index is not the Property order
    LogAttribute? LogAttribute,
    ImmutableList<RequirementAttribute> RequiredVersions) : ISerializable
{
    /// <summary>
    /// The Name of this StepProperty
    /// </summary>
    public string Name => StepParameter.Name;

    /// <summary>
    /// A lambda function
    /// </summary>
    public record LambdaFunctionProperty
    (
        LambdaFunction LambdaFunction,
        StepParameter StepParameter,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        StepParameter,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override TextLocation? MaybeTextLocation() => LambdaFunction.Step.TextLocation;

        /// <inheritdoc />
        public override string Serialize(SerializeOptions options)
        {
            return $"({LambdaFunction.Serialize(options)})";
        }

        /// <inheritdoc />
        public override void Format(
            IndentationStringBuilder indentationStringBuilder,
            FormattingOptions options,
            Stack<Comment> remainingComments)
        {
            indentationStringBuilder.Append("(");
            LambdaFunction.Format(indentationStringBuilder, options, remainingComments);
            indentationStringBuilder.Append(")");
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {Serialize(SerializeOptions.Serialize)}";
    }

    /// <summary>
    /// A variable name
    /// </summary>
    public record VariableNameProperty(
        VariableName VariableName,
        StepParameter StepParameter,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        StepParameter,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override TextLocation? MaybeTextLocation() => null;

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {VariableName}";

        /// <inheritdoc />
        public override string Serialize(SerializeOptions options) =>
            VariableName.Serialize(options);

        /// <inheritdoc />
        public override void Format(
            IndentationStringBuilder indentationStringBuilder,
            FormattingOptions options,
            Stack<Comment> remainingComments)
        {
            indentationStringBuilder.Append(Serialize(SerializeOptions.Serialize));
        }
    }

    /// <summary>
    /// A single step
    /// </summary>
    public record SingleStepProperty(
        IStep Step,
        StepParameter StepParameter,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        StepParameter,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override TextLocation? MaybeTextLocation() => Step.TextLocation;

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {Step}";

        /// <inheritdoc />
        public override string Serialize(SerializeOptions options)
        {
            var v = Step.Serialize(options);

            if (Step.ShouldBracketWhenSerialized)
                return $"({v})";

            return v;
        }

        /// <inheritdoc />
        public override void Format(
            IndentationStringBuilder indentationStringBuilder,
            FormattingOptions options,
            Stack<Comment> remainingComments)
        {
            if (Step.ShouldBracketWhenSerialized)
                indentationStringBuilder.Append("(");

            Step.Format(
                indentationStringBuilder,
                options,
                remainingComments
            );

            if (Step.ShouldBracketWhenSerialized)
                indentationStringBuilder.Append(")");
        }
    }

    /// <summary>
    /// A property which is a list of steps
    /// </summary>
    public record StepListProperty(
        IReadOnlyList<IStep> StepList,
        StepParameter StepParameter,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        StepParameter,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string ToString() => $"{Name} = {StepList}";

        /// <inheritdoc />
        public override string Serialize(SerializeOptions options)
        {
            var l = StepList.Select(
                    s =>
                    {
                        var ser = s.Serialize(options);

                        if (s.ShouldBracketWhenSerialized)
                            return $"({ser})";

                        return ser;
                    }
                )
                .ToList();

            return SerializationMethods.SerializeList(l);
        }

        /// <inheritdoc />
        public override void Format(
            IndentationStringBuilder indentationStringBuilder,
            FormattingOptions options,
            Stack<Comment> remainingComments)
        {
            indentationStringBuilder.Append("[");

            indentationStringBuilder.AppendJoin(
                ", ",
                false,
                StepList,
                step => step.Format(
                    indentationStringBuilder,
                    options,
                    remainingComments
                )
            );

            indentationStringBuilder.Append("]");
        }

        /// <inheritdoc />
        public override TextLocation? MaybeTextLocation() => null;
    }

    /// <summary>
    /// Serialize this
    /// </summary>
    public abstract string Serialize(SerializeOptions options);

    /// <inheritdoc />
    public abstract void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments);

    /// <summary>
    /// Get the text location if present
    /// </summary>
    public abstract TextLocation? MaybeTextLocation();
}
