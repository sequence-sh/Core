﻿using System;

namespace Reductech.EDR.Core.Attributes
{
    /// <summary>
    /// Indicates that this is a list of configurable properties of the step.
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StepListPropertyAttribute : StepPropertyBaseAttribute { }
}