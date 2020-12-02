namespace Reductech.EDR.Core.Internal
{
    public sealed class Enumeration
    {
        //TODO make a record
        public Enumeration(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }
        public string Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Type + "." + Value;
        }
    }
}