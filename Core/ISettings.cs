using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// External settings for running the step.
    /// </summary>
    public interface ISettings
    {

        /// <summary>
        /// Check that the requirement is met by these settings.
        /// </summary>
        Result<Unit, IErrorBuilder> CheckRequirement(Requirement requirement);
    }
}