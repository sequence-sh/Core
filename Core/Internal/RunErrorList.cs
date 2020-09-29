using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A list of errors thrown by a running step.
    /// </summary>
    public class RunErrorList : IRunErrors
    {
        /// <summary>
        /// Create a new RunErrorList
        /// </summary>
        /// <param name="allErrors"></param>
        public RunErrorList(IReadOnlyCollection<IRunError> allErrors) => AllErrors = allErrors;

        /// <inheritdoc />
        public IEnumerable<IRunError> AllErrors { get; }

        /// <inheritdoc />
        public string AsString =>
            string.Join("; ", AllErrors.Select(x => x.AsString));

        /// <summary>
        /// Combine multiple run errors.
        /// </summary>
        public static RunErrorList Combine(IEnumerable<IRunErrors> source) => new RunErrorList(source.SelectMany(x=>x.AllErrors).ToList());
    }
}