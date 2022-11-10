using System.Text;

namespace Sequence.Core.Internal.Serialization;

/// <summary>
/// A string builder that can keep track of indentation
/// </summary>
public class IndentationStringBuilder
{
    /// <summary>
    /// The current number of tabs to index
    /// </summary>
    public int Indentation { get; private set; } = 0;

    /// <summary>
    /// Indent the current line one tab
    /// </summary>
    public void Indent() => Indentation++;

    /// <summary>
    /// Unindent the current line one tab
    /// </summary>
    public void UnIndent() => Indentation--;

    /// <summary>
    /// Append text to th current line and then start a new line
    /// </summary>
    public void AppendLine(string line)
    {
        _current.Append(line);
        AppendLine();
    }

    /// <summary>
    /// Start a new line
    /// </summary>
    public void AppendLine()
    {
        _lines.Add((_current, Indentation));
        _current = new StringBuilder();
    }

    /// <summary>
    /// Start a new line
    /// </summary>
    public void AppendLineMaybe()
    {
        if (_current.Length > 0)
        {
            _lines.Add((_current, Indentation));
            _current = new StringBuilder();
        }
    }

    /// <summary>
    /// Append text to the current line
    /// </summary>
    public void Append(string text)
    {
        _current.Append(text);
    }

    /// <summary>
    /// Append several elements separated by a separator
    /// </summary>
    public void AppendJoin<T>(
        string separator,
        bool separateWithNewLines,
        IEnumerable<T> things,
        Action<T> action)
    {
        var first = true;

        foreach (var thing in things)
        {
            if (first)
                first = false;

            else if (separateWithNewLines)
            {
                Append(separator);
                AppendLineMaybe();
            }
            else
                Append(separator);

            action(thing);
        }
    }

    private readonly List<(StringBuilder words, int indentation)> _lines = new();

    private StringBuilder _current = new();

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var (line, indentation) in _lines)
        {
            if (indentation > 0)
                sb.Append(new string('\t', indentation));

            sb.AppendJoin("", line.ToString().Trim());
            sb.AppendLine();
        }

        if (_current.Length > 0)
        {
            sb.Append(new string('\t', Indentation));
            sb.Append(_current.ToString().Trim());
        }

        return sb.ToString();
    }
}
