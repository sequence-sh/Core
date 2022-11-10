namespace Sequence.Core.Internal.Serialization;

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
