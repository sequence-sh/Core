namespace Reductech.Sequence.Core.Internal.Errors;

/// <summary>
/// Multiple errors.
/// </summary>
public class ErrorList : IError
{
    /// <summary>
    /// Create a new ErrorList
    /// </summary>
    /// <param name="allErrors"></param>
    public ErrorList(IReadOnlyCollection<SingleError> allErrors) => _allErrors = allErrors;

    private readonly IReadOnlyCollection<SingleError> _allErrors;

    /// <inheritdoc />
    public IEnumerable<SingleError> GetAllErrors() => _allErrors;

    /// <inheritdoc />
    public string AsString => string.Join("; ", _allErrors.Select(x => x.AsString));

    /// <inheritdoc />
    public string AsStringWithLocation => string.Join(
        "; ",
        _allErrors.Select(x => x.AsStringWithLocation)
    );

    /// <inheritdoc />
    public IErrorBuilder ToErrorBuilder
    {
        get
        {
            return ErrorBuilderList.Combine(
                _allErrors.SelectMany(x => x.GetAllErrors())
                    .Select(x => x.ErrorBuilder)
            );
        }
    }

    /// <summary>
    /// Combine multiple run errors.
    /// </summary>
    public static IError Combine(IEnumerable<IError> source)
    {
        var errors = source.SelectMany(x => x.GetAllErrors()).Distinct().ToList();

        if (errors.Count == 1)
            return errors.Single();

        return new ErrorList(errors);
    }

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <inheritdoc />
    public bool Equals(IError? other) =>
        other != null && _allErrors.SequenceEqual(other.GetAllErrors());

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj is IError e && Equals(e);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (_allErrors.Count == 0)
            return 0;

        if (_allErrors.Count == 1)
            return _allErrors.First().GetHashCode();

        return HashCode.Combine(
            _allErrors.Count,
            _allErrors.First().GetHashCode(),
            _allErrors.Last().GetHashCode()
        );
    }
}
