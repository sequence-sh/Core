﻿using Reductech.EDR.Core.Serialization;
using Superpower.Model;
using YamlDotNet.Core;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// Extension methods for ErrorBuilders that involve adding a location.
    /// </summary>
    public static class ErrorLocationHelper
    {
        /// <summary>
        /// Add a StepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStep step)=> errorBuilder.WithLocation(new StepErrorLocation(step));

        /// <summary>
        /// Add a FreezableStepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IFreezableStep step)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(step));

        /// <summary>
        /// Add a FreezableStepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStepFactory factory, FreezableStepData data)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(factory, data));

        /// <summary>
        /// Add a YamlRegionErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, Mark start, Mark end, Position? position = null) =>
            errorBuilder.WithLocation(new YamlRegionErrorLocation(start, end, position));
    }
}