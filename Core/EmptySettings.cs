using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{

/// <summary>
/// A settings object with no fields.
/// </summary>
public record EmptySettings() : SCLSettings(
    new SCLSettingsValue.Map(ImmutableDictionary<string, SCLSettingsValue>.Empty)
)
{
    /// <summary>
    /// Gets the instance of EmptySettings.
    /// </summary>
    public static readonly SCLSettings Instance = new EmptySettings();

    /// <inheritdoc />
    public Result<Unit, IErrorBuilder> CheckRequirement(Requirement requirement) =>
        new ErrorBuilder(ErrorCode.RequirementNotMet, requirement);
}

}
