//using System;
//using Reductech.EDR.Core.Internal.Errors;

//namespace Reductech.EDR.Core.Serialization
//{
//    public class Mark
//    {
//        public int Line { get; }

//        public int CharPositionInLine { get; }
//    }

//    /// <summary>
//    /// An error location that contains the relative position in a yaml string where the error occured.
//    /// </summary>
//    public class YamlRegionErrorLocation : IErrorLocation
//    {
//        /// <summary>
//        /// Create a new YamlRegionErrorLocation
//        /// </summary>
//        public YamlRegionErrorLocation(Mark start, Mark end)
//        {
//            throw new NotImplementedException();
//            //if (position == null)
//            //    Start = start;
//            //else
//            //{
//            //    var errorLine = Math.Max(position.Value.Line - 1, 0);
//            //    var errorColumn = Math.Max(position.Value.Column - 1, 0);
//            //    var errorAbsolute = position.Value.Absolute;


//            //    Start = new Mark(start.Index + errorAbsolute,
//            //        start.Line + errorLine,
//            //        start.Column + errorColumn);
//            //}

//            End = end;
//        }

//        /// <summary>
//        /// The beginning of the region.
//        /// </summary>
//        public Mark Start { get; }

//        /// <summary>
//        /// The end of the region.
//        /// </summary>
//        public Mark End { get; }

//        /// <inheritdoc />
//        public string AsString => $"{Start} - {End}";

//        /// <inheritdoc />
//        public bool Equals(IErrorLocation? other)
//        {
//            if (other is null) return false;
//            if (ReferenceEquals(this, other)) return true;
//            return other is YamlRegionErrorLocation y && Start.Equals(y.Start) && End.Equals(y.End);
//        }

//        /// <inheritdoc />
//        public override bool Equals(object? obj)
//        {
//            if (obj is null) return false;
//            if (ReferenceEquals(this, obj)) return true;
//            return obj is IErrorLocation errorLocation && Equals(errorLocation);
//        }

//        /// <inheritdoc />
//        public override int GetHashCode() => HashCode.Combine(Start, End);
//    }
//}