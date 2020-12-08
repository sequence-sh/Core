using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A property of a step
    /// </summary>
    public class StepProperty
    {
        /// <summary>
        /// Create a new StepProperty
        /// </summary>
        public StepProperty(string name, int index, OneOf<VariableName, IStep, IReadOnlyList<IStep>> value)
        {
            Name = name;
            Index = index;
            Value = value;
        }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get;  }
        /// <summary>
        /// The position of this property among the other properties
        /// </summary>
        public int Index { get;  }

        /// <summary>
        /// The value of the property
        /// </summary>
        public OneOf<VariableName, IStep, IReadOnlyList<IStep>> Value { get;  }

        /// <inheritdoc />
        public override string ToString() => Name + " = " + Value.Match(x=>x.ToString(), x=>x.ToString(), x=>x.ToString());

        /// <summary>
        /// Serialize the property value
        /// </summary>
        public async Task<string> SerializeValueAsync(CancellationToken cancellationToken)
        {
            var result = await
            Value.Match<Task<string>> (vn => Task.FromResult(vn.Serialize()),
                async x =>
                {
                    var r = await x.SerializeAsync(cancellationToken);
                    return r;
                },
                async l =>
                {
                    var r = await SerializeList(l);
                    return r;
                });

            return result;

            async Task<string> SerializeList(IReadOnlyList<IStep> list)
            {
                var l = new List<string>();

                foreach (var s in list)
                {
                    var r = await s.SerializeAsync(cancellationToken);
                    l.Add(r);
                }

                return SerializationMethods.SerializeList(l);
            }
        }

    }
}