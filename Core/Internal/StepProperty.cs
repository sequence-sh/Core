using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A property of a step
/// </summary>
public abstract record StepProperty
(
    string Name,
    int Index,
    LogAttribute? LogAttribute,
    ScopedFunctionAttribute? ScopedFunctionAttribute,
    ImmutableList<RequirementAttribute> RequiredVersions)
{
    /// <summary>
    /// A lambda function
    /// </summary>
    public record LambdaFunctionProperty
    (
        LambdaFunction LambdaFunction,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequiredVersionAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        protected override string SerializeValue()
        {
            return LambdaFunction.Serialize();
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {SerializeValue()}";

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized => true;

        /// <inheritdoc />
        public override string GetLogName()
        {
            return SerializeValue();
        }
    }

    /// <summary>
    /// A variable name
    /// </summary>
    public record VariableNameProperty(
        VariableName VariableName,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ScopedFunctionAttribute? ScopedFunctionAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string GetLogName() => VariableName.ToString();

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {VariableName}";

        /// <inheritdoc />
        protected override string SerializeValue() => VariableName.Serialize();

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized { get; } = false;
    }

    /// <summary>
    /// A single step
    /// </summary>
    public record SingleStepProperty(
        IStep Step,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ScopedFunctionAttribute? ScopedFunctionAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string GetLogName()
        {
            return Step switch
            {
                StringConstant str => str.Value.NameInLogs(
                    LogAttribute?.LogOutputLevel != LogOutputLevel.None
                ),
                IConstantStep constant            => constant.Name,
                CreateEntityStep createEntityStep => createEntityStep.Name,
                ICompoundStep                     => Step.Name,
                _                                 => Step.Name
            };
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {Step}";

        /// <inheritdoc />
        protected override string SerializeValue() => Step.Serialize();

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized { get; } =
            Step.ShouldBracketWhenSerialized;
    }

    /// <summary>
    /// A property which is a list of steps
    /// </summary>
    public record StepListProperty(
        IReadOnlyList<IStep> StepList,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ScopedFunctionAttribute? ScopedFunctionAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string GetLogName()
        {
            return StepList.Count + " Elements";
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name} = {StepList}";

        /// <inheritdoc />
        protected override string SerializeValue()
        {
            var l = StepList.Select(
                    s =>
                    {
                        var ser = s.Serialize();

                        if (s.ShouldBracketWhenSerialized)
                        {
                            return $"({ser})";
                        }

                        return ser;
                    }
                )
                .ToList();

            return SerializationMethods.SerializeList(l);
        }

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized { get; } = false;
    }

    /// <summary>
    /// Get the serialized value
    /// </summary>
    protected abstract string SerializeValue();

    /// <summary>
    /// Whether the value should be bracketed when it was serialized
    /// </summary>
    protected abstract bool ShouldBracketWhenSerialized { get; }

    /// <summary>
    /// Serialize this
    /// </summary>
    public string Serialize()
    {
        var v = SerializeValue();

        if (ShouldBracketWhenSerialized)
            return $"({v})";

        return v;
    }

    /// <summary>
    /// Gets the name as it will appear in the log file.
    /// </summary>
    /// <returns></returns>
    public abstract string GetLogName();
}

}
