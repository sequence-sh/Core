namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// A location with no information
    /// </summary>
    public class EmptyLocation : IErrorLocation
    {
        private EmptyLocation() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IErrorLocation Instance { get; } = new EmptyLocation();

        /// <inheritdoc />
        public bool Equals(IErrorLocation? other) => other is EmptyLocation;

        /// <inheritdoc />
        public string AsString => "No Location";
    }

    ///// <summary>
    ///// An error location composed of multiple locations.
    ///// </summary>
    //public class ErrorLocationList : IErrorLocation
    //{
    //    public ErrorLocationList(ImmutableList<IErrorLocation> locations) => Locations = locations;

    //    /// <summary>
    //    /// The ErrorLocations
    //    /// </summary>
    //    public ImmutableList<IErrorLocation> Locations { get; }


    //    /// <inheritdoc />
    //    public bool Equals(IErrorLocation? other)
    //    {
    //        if (other is ErrorLocationList cel && Locations.SequenceEqual(cel.Locations))
    //            return true;

    //        if (Locations.Count == 1)
    //            return Locations.Single().Equals(other);

    //        return false;
    //    }

    //    /// <inheritdoc />
    //    public string AsString => string.Join("; ", Locations.Select(x => x.AsString));

    //    /// <summary>
    //    /// Combine several ErrorLocations into a single ErrorLocation.
    //    /// </summary>
    //    public static IErrorLocation Combine(IEnumerable<IErrorLocation> locations)
    //    {
    //        var list = locations
    //            .SelectMany(x => x is ErrorLocationList ell ? ell.Locations : ImmutableList.Create(x))
    //            .Where(x=> x != EmptyLocation.Instance)
    //            .ToImmutableList();

    //        return list.Count switch
    //        {
    //            0 => EmptyLocation.Instance,
    //            1 => list.Single(),
    //            _ => new ErrorLocationList(list)
    //        };
    //    }
    //}


    /// <summary>
    /// The error location was the entire sequence.
    /// </summary>
    public class EntireSequenceLocation : IErrorLocation
    {
        private EntireSequenceLocation() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static IErrorLocation Instance { get; } = new EntireSequenceLocation();

        /// <inheritdoc />
        public string AsString => "Entire Sequence";

        /// <inheritdoc />
        public bool Equals(IErrorLocation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is EntireSequenceLocation;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IErrorLocation errorLocation && Equals(errorLocation);
        }

        /// <inheritdoc />
        public override int GetHashCode() => 42;
    }
}