using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Deserializes text to primitive types.
    /// </summary>
    public static class SerializationMethods
    {
        /// <summary>
        /// Serialize a constant freezable process.
        /// </summary>
        public static string SerializeConstant(ConstantFreezableProcess cfp, bool quoteString)
        {
            if (cfp.Value.GetType().IsEnum)
                return cfp.Value.GetType().Name + "." + cfp.Value;
            if (cfp.Value is string s)
                return quoteString? $"'{s}'" : s;
            return cfp.Value.ToString() ?? "";
        }
    }
}
