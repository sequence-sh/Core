using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// An error without a location.
    /// </summary>
    public interface IErrorBuilder
    {
        /// <summary>
        /// Converts this errorBuilder to an error
        /// </summary>
        public IError WithLocation(IErrorLocation location);

        /// <summary>
        /// The error builders.
        /// </summary>
        public IEnumerable<ErrorBuilder> GetErrorBuilders();

        public string AsString { get; }
    }
}