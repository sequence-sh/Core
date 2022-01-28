using System.Text;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Options for formatting an scl object
/// </summary>
public record FormattingOptions;

/// <summary>
/// Indicates an object that can serialized and formatted
/// </summary>
public interface ISerializable
{
    /// <summary>
    /// Serialize this object
    /// </summary>
    [Pure]
    string Serialize(SerializeOptions options);

    /// <summary>
    /// Format this object as a multiline indented string
    /// </summary>
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments);
}

/// <summary>
/// Contains methods to help print comments at the right time
/// </summary>
public static class CommentHelper
{
    /// <summary>
    /// Print all comments that should be printed before this location
    /// </summary>
    public static void AppendPrecedingComments(
        this IndentationStringBuilder sb,
        Stack<Comment> comments,
        TextLocation? location)
    {
        if (location is null)
            return;

        while (ShouldPrintCommentBefore(comments, location, out var comment))
            comment!.Append(sb);

        static bool ShouldPrintCommentBefore(
            Stack<Comment> comments,
            TextLocation textLocation,
            out Comment? comment)
        {
            if (!comments.TryPeek(out comment))
                return false;

            if (comment.Position.Index < textLocation?.Start.Index)
            {
                comments.Pop();
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Print all comments that belong within this location
    /// </summary>
    public static void AppendContainingComments(
        this IndentationStringBuilder sb,
        Stack<Comment> comments,
        TextLocation? location)
    {
        if (location is null)
            return;

        while (ShouldPrintCommentBeforeEnd(comments, location, out var comment))
            comment!.Append(sb);

        static bool ShouldPrintCommentBeforeEnd(
            Stack<Comment> comments,
            TextLocation textLocation,
            out Comment? comment)
        {
            if (!comments.TryPeek(out comment))
                return false;

            if (comment.Position.Index < textLocation?.Stop.Index)
            {
                comments.Pop();
                return true;
            }

            return false;
        }
    }
}

/// <summary>
/// A piece of comment text
/// </summary>
public record Comment(string Text, bool IsSingleLine, TextPosition Position)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    public void Append(IndentationStringBuilder sb)
    {
        sb.Append(Text);

        if (IsSingleLine)
            sb.AppendLine();
    }
}

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
