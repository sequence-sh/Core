using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Deserializes text to primitive types.
    /// </summary>
    public static class SerializationMethods
    {
        /// <summary>
        /// Serialize a constant freezable step.
        /// </summary>
        public static string SerializeConstant(ConstantFreezableStep cfp, bool quoteString)
        {
            if (cfp.Value.GetType().IsEnum)
                return cfp.Value.GetType().Name + "." + cfp.Value;
            if (cfp.Value is string s)
                return quoteString? $"'{s}'" : s;
            return cfp.Value.ToString() ?? "";
        }
    }
}
