using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{
    /// <summary>
    /// Makes sure all steps have a test class
    /// </summary>
    public class MetaTests : MetaTestsBase
    {
        /// <inheritdoc />
        public override Assembly StepAssembly => typeof(IStep).Assembly;

        /// <inheritdoc />
        public override Assembly TestAssembly => typeof(MetaTests).Assembly;
    }
}
