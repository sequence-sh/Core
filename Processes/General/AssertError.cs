using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns success if the Test process returns an error and a failure otherwise.
    /// </summary>
    public sealed class AssertErrorProcessFactory : SimpleRunnableProcessFactory<AssertError, Unit>
    {
        private AssertErrorProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<AssertError, Unit> Instance { get; } = new AssertErrorProcessFactory();
    }

    /// <summary>
    /// Returns success if the Test process returns an error and a failure otherwise.
    /// </summary>
    public sealed class AssertError : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var result = Test.Run(processState);

            if (result.IsFailure)
                return Unit.Default;

            return new RunError("Expected an error but process was successful.", Name, null, ErrorCode.AssertionFailed);
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => AssertErrorProcessFactory.Instance;

        /// <summary>
        /// The process to test.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<Unit> Test { get; set; } = null!;
    }
}
