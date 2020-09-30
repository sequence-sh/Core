using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns an error if the nested step does not return true.
    /// </summary>
    public sealed class AssertTrue : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad) =>
            Test.Run(stateMonad).Ensure(x => x,
                new RunError($"Assertion Failed '{Test.Name}'", Name, null, ErrorCode.IndexOutOfBounds)).Map(x=> Unit.Default);

        /// <inheritdoc />
        public override IStepFactory StepFactory => AssertTrueStepFactory.Instance;

        /// <summary>
        /// The step to test.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Test { get; set; } = null!;
    }

    /// <summary>
    /// Returns an error if the nested step does not return true.
    /// </summary>
    public sealed class AssertTrueStepFactory : SimpleStepFactory<AssertTrue, Unit>
    {
        private AssertTrueStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AssertTrue, Unit> Instance { get; } = new AssertTrueStepFactory();
    }
}
