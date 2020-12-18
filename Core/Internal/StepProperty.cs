using System.Collections.Generic;
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
        public string Serialize()
        {
            var result =
            Value.Match (vn => vn.Serialize(),
                 x =>
                {
                    var r = x.Serialize();
                    return r;
                },
                 l =>
                {
                    var r = SerializeList(l);
                    return r;
                });


            if (Value.IsT1 && Value.AsT1.ShouldBracketWhenSerialized)
                result = "(" + result + ")";

            return result;

            string SerializeList(IReadOnlyList<IStep> list)
            {
                var l = new List<string>();

                foreach (var s in list)
                {
                    var r = s.Serialize();
                    l.Add(r);
                }

                return SerializationMethods.SerializeList(l);
            }
        }

    }
}