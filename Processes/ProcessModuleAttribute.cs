﻿using System;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Use this attribute if your solution contains orchestration processes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]

    public sealed class ProcessModuleAttribute : Attribute
    {

    }
}