using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// A settings object with no fields.
    /// </summary>
    public class EmptySettings : ISettings
    {
        /// <summary>
        /// Gets the instance of EmptySettings.
        /// </summary>
        public static ISettings Instance = new EmptySettings();


        private EmptySettings() {}

        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> CheckRequirement(Requirement requirement) =>
            new ErrorBuilder($"Requirement '{requirement}' not met.",
                ErrorCode.RequirementsNotMet);
    }
}