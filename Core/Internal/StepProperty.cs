using System.Collections.Generic;
using System.Linq;
using OneOf;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A property of a step
    /// </summary>
    public class StepProperty
    {
        public StepProperty(string name, int index, OneOf<VariableName, IStep, IReadOnlyList<IStep>> value)
        {
            Name = name;
            Index = index;
            Value = value;
        }

        public string Name { get;  }
        public int Index { get;  }
        public OneOf<VariableName, IStep, IReadOnlyList<IStep>> Value { get;  }

        /// <inheritdoc />
        public override string ToString() => Name + " = " + SerializeValue();

        /// <summary>
        /// Serialize the property value
        /// </summary>
        public string SerializeValue()
        {
            return Value.Match(vn=>vn.Serialize(),
                x => x.Serialize(),
                SerializeList);

            static string SerializeList(IReadOnlyList<IStep> list)
            {
                var elements = list.Select(x=>x.Serialize());

                return SerializationMethods.SerializeList(elements);
            }
        }

    }
}