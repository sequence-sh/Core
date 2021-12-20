﻿namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// Contributes to the serialized string
/// </summary>
public interface ISerializerBlock
{
    /// <summary>
    /// Gets the segment of serialized text if possible
    /// </summary>
    public Result<string> TryGetSegmentText(
        SerializeOptions options,
        IReadOnlyDictionary<string, StepProperty> dictionary);
}
