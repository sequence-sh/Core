namespace Reductech.EDR.Core.Serialization
{
    internal class YamlString
    {
        public YamlString(string value) { Value = value; }

        public string Value { get; }

        /// <inheritdoc />
        public override string ToString() => Value;
    }
}