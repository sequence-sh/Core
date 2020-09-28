using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
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
}
