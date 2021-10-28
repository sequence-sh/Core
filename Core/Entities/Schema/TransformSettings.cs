﻿using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Entities.Schema
{

/// <summary>
/// Settings for using a Schema to Transform a value
/// </summary>
public record TransformSettings(
    Formatter DateFormatter,
    Formatter TruthFormatter,
    Formatter FalseFormatter,
    Formatter NullFormatter,
    Formatter MultiValueFormatter,
    bool CaseSensitive,
    Maybe<bool> RemoveExtra) { }

}