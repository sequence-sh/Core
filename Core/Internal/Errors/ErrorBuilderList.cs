using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// More than one errorBuilders
    /// </summary>
    public class ErrorBuilderList : IErrorBuilder
    {
        /// <summary>
        /// Create a new ErrorBuilderList
        /// </summary>
        public ErrorBuilderList(IEnumerable<ErrorBuilder> errorBuilders) => ErrorBuilders = errorBuilders.ToList();

        /// <summary>
        /// The errorBuilders
        /// </summary>
        public IReadOnlyCollection<ErrorBuilder> ErrorBuilders { get; }

        /// <inheritdoc />
        public IError WithLocation(IErrorLocation location) => new ErrorList(ErrorBuilders.Select(x=>x.WithLocationSingle(location)).ToList());

        /// <inheritdoc />
        public IEnumerable<ErrorBuilder> GetErrorBuilders() => ErrorBuilders;

        /// <inheritdoc />
        public string AsString => string.Join("; ", ErrorBuilders.Select(x => x.Message));

        /// <inheritdoc />
        public override string ToString() => AsString;

        /// <summary>
        /// Combine multiple ErrorBuilders
        /// </summary>
        public static IErrorBuilder Combine(IEnumerable<IErrorBuilder> errorBuilders) => new ErrorBuilderList(errorBuilders.SelectMany(x=>x.GetErrorBuilders()));
    }
}