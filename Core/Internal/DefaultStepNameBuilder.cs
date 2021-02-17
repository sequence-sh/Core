using System.Linq;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// The default step name builder
/// </summary>
public class DefaultStepNameBuilder : IStepNameBuilder
{
    private DefaultStepNameBuilder() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static DefaultStepNameBuilder Instance { get; } = new();

    /// <inheritdoc />
    public string GetFromArguments(FreezableStepData freezableStepData, IStepFactory stepFactory)
    {
        var args = string.Join(
            ", ",
            freezableStepData
                .StepProperties
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}: {x.Value.MemberString}")
        );

        return $"{stepFactory.TypeName}({args})";
    }
}

}
