namespace Sequence.Core.Internal;

/// <summary>
/// A lambda function with typed parameters
/// </summary>
public record LambdaFunction<TInput, TOutput> : LambdaFunction
    where TInput : ISCLObject
    where TOutput : ISCLObject
{
    /// <summary>
    /// The Step to execute
    /// </summary>
    public IStep<TOutput> StepTyped { get; }

    /// <summary>
    /// A lambda function
    /// </summary>
    public LambdaFunction(VariableName? variable, IStep<TOutput> stepTyped) : base(
        variable,
        stepTyped
    )
    {
        StepTyped = stepTyped;
    }
}

/// <summary>
/// A lambda function
/// </summary>
public record LambdaFunction : ISerializable
{
    /// <summary>
    /// A lambda function
    /// </summary>
    protected LambdaFunction(VariableName? variable, IStep step)
    {
        Variable = variable;
        Step     = step;
    }

    /// <summary>
    /// Serialize this Lambda function
    /// </summary>
    public string Serialize(SerializeOptions options)
    {
        var stepSerialized = Step.Serialize(options);

        if (Variable is null)
        {
            return $"<> => {stepSerialized}";
        }

        return $"{Variable.Value.Serialize(options)} => {stepSerialized}";
    }

    /// <inheritdoc />
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        var variable = Variable is null
            ? "<>"
            : Variable.Value.Serialize(SerializeOptions.Serialize);

        indentationStringBuilder.Append($"{variable} => ");
        indentationStringBuilder.Indent();
        Step.Format(indentationStringBuilder, options, remainingComments);
        indentationStringBuilder.UnIndent();
    }

    /// <summary>
    /// The VariableName or The Item variable name
    /// </summary>
    public VariableName VariableNameOrItem => Variable ?? VariableName.Item;

    /// <summary>
    /// The VariableName to use inside the function
    /// </summary>
    public VariableName? Variable { get; init; }

    /// <summary>
    /// The step
    /// </summary>
    public IStep Step { get; init; }
}
