using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{

    /// <summary>
    /// Deserializes a regex group into an enum using the display value of the enum.
    /// </summary>
    public class EnumDisplayComponent<T> : ISerializerBlock where T : Enum
    {
        /// <summary>
        /// Creates a new EnumDisplayComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public EnumDisplayComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public async Task<Result<string>> TryGetSegmentTextAsync(IReadOnlyDictionary<string, StepProperty> dictionary,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return dictionary.TryFindOrFail(PropertyName, $"Missing Property {PropertyName}")
                .Bind(x => x.Value.Match(
                    _ => Result.Failure<string>("Operator is VariableName"),
                    s=> s is EnumConstant<T> cs? cs.Value.GetDisplayName() : Result.Failure<string>("Operator is non constant step"),
                    sl => Result.Failure<string>("Operator is Step List")


                ));
        }
    }
}