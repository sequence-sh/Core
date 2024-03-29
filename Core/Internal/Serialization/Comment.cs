﻿namespace Sequence.Core.Internal.Serialization;

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

    /// <summary>
    /// Append another comment
    /// </summary>
    public Comment Append(Comment other)
    {
        return new(
            Text + "\r\n" + other.Text,
            false,
            Position.Index < other.Position.Index ? Position : other.Position
        );
    }
}
