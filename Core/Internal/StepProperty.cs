using System.Collections.Generic;
using System.Linq;
using OneOf;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A property of a step
    /// </summary>
    public class StepProperty : OneOfBase<VariableName, IStep, IReadOnlyList<IStep>>
    {
        /// <summary>
        /// Create a new StepProperty
        /// </summary>
        public StepProperty(string name,
            int index,
            OneOf<VariableName, IStep, IReadOnlyList<IStep>> value,
            LogAttribute? logAttribute,
            ScopedFunctionAttribute? scopedFunctionAttribute) : base(value)
        {
            Name = name;
            Index = index;
            LogAttribute = logAttribute;
            ScopedFunctionAttribute = scopedFunctionAttribute;
        }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get;  }
        /// <summary>
        /// The position of this property among the other properties
        /// </summary>
        public new int Index { get;  }

        /// <summary>
        /// An attribute that says whether to log this property
        /// </summary>
        public LogAttribute? LogAttribute { get; }
        /// <summary>
        /// An attribute that indicates this is a scoped function property
        /// </summary>
        public ScopedFunctionAttribute? ScopedFunctionAttribute { get; }

        /// <inheritdoc />
        public override string ToString() => Name + " = " + Match(x=>x.ToString(), x=>x.ToString(), x=>x.ToString());

        /// <summary>
        /// Serialize the property value
        /// </summary>
        public string Serialize()
        {
            var result =
            Match (vn => vn.Serialize(),
                 x =>
                {
                    var r = x.Serialize();
                    return r;
                },
                 l =>
                {
                    var r = SerializeEnumerable(l);
                    return r;
                });


            if (IsT1 && AsT1.ShouldBracketWhenSerialized)
                result = "(" + result + ")";

            return result;

            static string SerializeEnumerable(IEnumerable<IStep> enumerable)
            {
                var l = enumerable.Select(s => s.Serialize()).ToList();

                return SerializationMethods.SerializeList(l);
            }
        }

        /// <summary>
        /// Get the name as it will appear in a log file.
        /// </summary>
        /// <returns></returns>
        public string GetLogName()
        {
            return Match(
                vn => vn.ToString(),
                step =>
                {
                    return step switch
                    {
                        StringConstant str => str.Value.NameInLogs(LogAttribute?.LogOutputLevel != LogOutputLevel.None),
                        IConstantStep constant => constant.Name,
                        CreateEntityStep createEntityStep => createEntityStep.Name,
                        ICompoundStep => step.Name,
                        _ => step.Name
                    };
                },
                GetListValue);

            static string GetListValue(IReadOnlyList<IStep> list)
            {
                return list.Count + " Elements";
            }
        }

    }
}