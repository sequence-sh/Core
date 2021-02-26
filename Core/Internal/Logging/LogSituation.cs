using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// Identifying code for the log situation.
/// </summary>
public abstract record LogSituationBase(string Code, LogLevel LogLevel)
{
    /// <summary>
    /// Log an occurrence of this situation
    /// </summary>
    public void Log(IStateMonad stateMonad, IStep? step, params object?[] args) =>
        stateMonad.Logger.LogSituation(this, step, stateMonad, args);

    /// <summary>
    /// Get the format string for this log situation
    /// </summary>
    protected abstract string GetLocalizedString();

    /// <summary>
    /// Gets a localized string for these arguments
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public (string message, object properties) GetLocalizedString(object?[] args)
    {
        if (_logValuesFormatter == null)
            _logValuesFormatter = new LogValuesFormatter(GetLocalizedString());

        var message = _logValuesFormatter.Format(args);

        var values = _logValuesFormatter.GetValues(args).ToDictionary(x => x.Key, x => x.Value);

        return (message, values);
    }

    private LogValuesFormatter? _logValuesFormatter;

    /// <summary>
    /// The Error Code.
    /// </summary>
    public string Code { get; init; } = Code;

    /// <summary>
    /// The log visibility level.
    /// </summary>
    public LogLevel LogLevel { get; init; } = LogLevel;

    internal class LogValuesFormatter
    {
        private static readonly char[] FormatDelimiters = { ',', ':' };
        private readonly string _format;
        private readonly List<string> _valueNames = new();

        public LogValuesFormatter(string format)
        {
            OriginalFormat = format;
            StringBuilder stringBuilder = new();
            var           startIndex    = 0;
            var           length        = format.Length;

            while (startIndex < length)
            {
                var braceIndex1 = FindBraceIndex(format, '{', startIndex,  length);
                var braceIndex2 = FindBraceIndex(format, '}', braceIndex1, length);

                if (braceIndex2 == length)
                {
                    stringBuilder.Append(format, startIndex, length - startIndex);
                    startIndex = length;
                }
                else
                {
                    var indexOfAny = FindIndexOfAny(
                        format,
                        FormatDelimiters,
                        braceIndex1,
                        braceIndex2
                    );

                    stringBuilder.Append(format, startIndex, braceIndex1 - startIndex + 1);
                    stringBuilder.Append(_valueNames.Count.ToString(CultureInfo.InvariantCulture));

                    _valueNames.Add(
                        format.Substring(braceIndex1 + 1, indexOfAny - braceIndex1 - 1)
                    );

                    stringBuilder.Append(format, indexOfAny, braceIndex2 - indexOfAny + 1);
                    startIndex = braceIndex2 + 1;
                }
            }

            _format = stringBuilder.ToString();
        }

        public string OriginalFormat { get; }

        public List<string> ValueNames => _valueNames;

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            var num1  = endIndex;
            var index = startIndex;
            var num2  = 0;

            for (; index < endIndex; ++index)
            {
                if (num2 > 0 && format[index] != brace)
                {
                    if (num2 % 2 == 0)
                    {
                        num2 = 0;
                        num1 = endIndex;
                    }
                    else
                        break;
                }
                else if (format[index] == brace)
                {
                    if (brace == '}')
                    {
                        if (num2 == 0)
                            num1 = index;
                    }
                    else
                        num1 = index;

                    ++num2;
                }
            }

            return num1;
        }

        private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
        {
            var num = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return num != -1 ? num : endIndex;
        }

        public string Format(object?[] values)
        {
            for (var index = 0; index < values.Length; ++index)
                values[index] = FormatArgument(values[index]);

            return string.Format(
                CultureInfo.InvariantCulture,
                _format,
                values
            );
        }

        internal string Format() => _format;

        internal string Format(object arg0) => string.Format(
            CultureInfo.InvariantCulture,
            _format,
            FormatArgument(arg0)
        );

        internal string Format(object arg0, object arg1) => string.Format(
            CultureInfo.InvariantCulture,
            _format,
            FormatArgument(arg0),
            FormatArgument(arg1)
        );

        internal string Format(object arg0, object arg1, object arg2) => string.Format(
            CultureInfo.InvariantCulture,
            _format,
            FormatArgument(arg0),
            FormatArgument(arg1),
            FormatArgument(arg2)
        );

        public KeyValuePair<string, object> GetValue(object[] values, int index)
        {
            if (index < 0 || index > _valueNames.Count)
                throw new IndexOutOfRangeException(nameof(index));

            return _valueNames.Count > index
                ? new KeyValuePair<string, object>(_valueNames[index], values[index])
                : new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues(object?[] values)
        {
            KeyValuePair<string, object>[] keyValuePairArray =
                new KeyValuePair<string, object>[values.Length + 1];

            for (var index = 0; index != _valueNames.Count; ++index)
                keyValuePairArray[index] = new KeyValuePair<string, object>(
                    _valueNames[index],
                    values[index] ?? NullString
                );

            keyValuePairArray[^1] =
                new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);

            return keyValuePairArray;
        }

        const string NullString = "(null)";

        private object FormatArgument(object? value)
        {
            return value switch
            {
                null     => NullString,
                string _ => value,
                IEnumerable source => string.Join(
                    ", ",
                    source.Cast<object>().Select(o => o ?? NullString)
                ),
                _ => value
            };
        }
    }
}

}
