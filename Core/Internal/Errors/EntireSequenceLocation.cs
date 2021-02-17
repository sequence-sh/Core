//using Reductech.EDR.Core.Internal.Parser;

//namespace Reductech.EDR.Core.Internal.Errors
//{

///// <summary>
///// A location with no information
///// </summary>
//public class EmptyLocation : IErrorLocation
//{
//    private EmptyLocation() { }

//    /// <summary>
//    /// The instance.
//    /// </summary>
//    public static IErrorLocation Instance { get; } = new EmptyLocation();

//    /// <inheritdoc />
//    public bool Equals(IErrorLocation? other) => other is EmptyLocation;

//    /// <inheritdoc />
//    public string AsString => "No Location";

//    /// <inheritdoc />
//    public string? StepName => null;

//    /// <inheritdoc />
//    public TextLocation? TextLocation => null;
//}

///// <summary>
///// The error location was the entire sequence.
///// </summary>
//public class EntireSequenceLocation : IErrorLocation
//{
//    private EntireSequenceLocation() { }

//    /// <summary>
//    /// The instance.
//    /// </summary>
//    public static IErrorLocation Instance { get; } = new EntireSequenceLocation();

//    /// <inheritdoc />
//    public string AsString => "Entire Sequence";

//    /// <inheritdoc />
//    public string? StepName => null;

//    /// <inheritdoc />
//    public TextLocation? TextLocation => null;

//    /// <inheritdoc />
//    public bool Equals(IErrorLocation? other)
//    {
//        if (other is null)
//            return false;

//        if (ReferenceEquals(this, other))
//            return true;

//        return other is EntireSequenceLocation;
//    }

//    /// <inheritdoc />
//    public override bool Equals(object? obj)
//    {
//        if (obj is null)
//            return false;

//        if (ReferenceEquals(this, obj))
//            return true;

//        return obj is IErrorLocation errorLocation && Equals(errorLocation);
//    }

//    /// <inheritdoc />
//    public override int GetHashCode() => 42;
//}

//}


