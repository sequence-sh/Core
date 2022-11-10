namespace Sequence.Core.Internal.Serialization;

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

    ///// <summary>
    ///// Get format components for this object
    ///// </summary>
    //[Pure]
    //FormatComponent GetFormatComponent(FormattingOptions options);

    //[Pure]
    //public string Format(FormattingOptions options, IEnumerable<Comment> comments)
    //{
    //    var component = GetFormatComponent(options);

    //    foreach (var comment in comments)
    //    {
    //        (_, component) = component.WithComment(comment);
    //    }

    //    return component.ToString(0, null);
    //}
}

///// <summary>
///// A component of string formatting
///// </summary>
//public abstract record FormatComponent(Comment? Comment)
//{
//    public record EntityPropertyComponent(
//        string Name,
//        FormatComponent Value,
//        Comment? Comment) : ParentComponent(Comment)
//    {
//        /// <inheritdoc />
//        public override string ToString(
//            int indentationLevel,
//            IReadOnlyList<FormatComponent>? siblings)
//        {
//            int afternamePadding;
//            int preCommentPadding;

//            if (siblings is null || siblings.Count == 0)
//            {
//                afternamePadding  = 0;
//                preCommentPadding = 1;
//            }
//            else
//            {
//                afternamePadding = siblings.OfType<EntityPropertyComponent>()
//                    .Select(x => x.Name.Length)
//                    .Max() - Name.Length;

//                preCommentPadding = siblings.OfType<EntityPropertyComponent>()
//                    .Select(x => x.ValueLength)
//                    .Aggregate(
//                        0 as int?,
//                        (a, x) => a.HasValue && x.HasValue ? Math.Max(a.Value, x.Value) : null
//                    ) ?? 0;

//                preCommentPadding += 1;
//            }

//            ret
//        }

//        /// <inheritdoc />
//        public override bool Contains(TextPosition textPosition) => Value.Contains(textPosition);

//        /// <inheritdoc />
//        public override bool ComesAfter(TextPosition textPosition) =>
//            Value.ComesAfter(textPosition);

//        /// <inheritdoc />
//        public override IEnumerable<FormatComponent> NestedComponents
//        {
//            get
//            {
//                yield return Value;
//            }
//        }

//        public int? ValueLength
//        {
//            get
//            {
//                if (this.Value is SingleValueComponent svc)
//                    return svc.Text.Length;

//                return null;
//            }
//        }

//        /// <inheritdoc />
//        public override FormatComponent WithChild(int index, FormatComponent child) =>
//            this with { Value = child };
//    }

//    /// <summary>
//    /// A component with a single string value
//    /// </summary>
//    public record SingleValueComponent
//        (string Text, Comment? Comment) : FormatComponent(Comment)
//    {
//        /// <inheritdoc />
//        public override string ToString(
//            int indentationLevel,
//            IReadOnlyList<FormatComponent>? siblings)
//        {
//            return new string('\t', indentationLevel) + Text;
//        }

//        /// <inheritdoc />
//        public override bool Contains(TextPosition textPosition) => false;

//        /// <inheritdoc />
//        public override bool ComesAfter(TextPosition textPosition) => false;
//    }

//    public abstract record ParentComponent(Comment? Comment) : FormatComponent(Comment)
//    {
//        public abstract IEnumerable<FormatComponent> NestedComponents { get; }

//        /// <summary>
//        /// Mutate a particular child of a component
//        /// </summary>
//        public abstract FormatComponent WithChild(int index, FormatComponent child);
//    }

//    public abstract string ToString(int indentationLevel, IReadOnlyList<FormatComponent>? siblings);

//    /// <inheritdoc />
//    public sealed override string ToString() => ToString(0, null);

//    public abstract bool Contains(TextPosition textPosition);
//    public abstract bool ComesAfter(TextPosition textPosition);

//    public (bool Changed, FormatComponent Component) WithComment(Comment newComment)
//    {
//        if (Contains(newComment.Position))
//        {
//            if (this is ParentComponent parent)
//            {
//                var nestedComponents = parent.NestedComponents.ToList();

//                for (var i = 0; i < nestedComponents.Count; i++)
//                {
//                    var nested = nestedComponents[i];

//                    if (nested.Contains(newComment.Position))
//                    {
//                        var (changed, newNested) = nested.WithComment(newComment);

//                        if (changed)
//                        {
//                            var newThis = parent.WithChild(i, newNested);
//                            return (true, newThis);
//                        }
//                    }

//                    if (nested.ComesAfter(newComment.Position))
//                    {
//                        if (i == 0)
//                        {
//                            var previous = nestedComponents[i - 1];
//                            var (changed, newNested) = previous.WithComment(newComment);

//                            if (changed)
//                            {
//                                var newThis = parent.WithChild(i - 1, newNested);
//                                return (true, newThis);
//                            }
//                        }

//                        break;
//                    }
//                }
//            }

//            {
//                var newThis =
//                    Comment is null
//                        ? this with { Comment = newComment }
//                        : this with { Comment = Comment.Append(newComment) };

//                return (true, newThis);
//            }
//        }

//        return (false, this);
//    }
//}
