using YamlDotNet.Core;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// The location in the yaml where the error occured.
    /// </summary>
    public class YamlErrorLocation : IErrorLocation
    {
        public YamlErrorLocation(YamlException yamlException) : this(yamlException.Start, yamlException.End)
        {
        }

        public YamlErrorLocation(Mark start, Mark end)
        {
            Start = start;
            End = end;
        }

        public Mark Start { get; }

        public Mark End { get; }

        /// <inheritdoc />
        public string AsString => $"{Start} - {End}";
    }
}