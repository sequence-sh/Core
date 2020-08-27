using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns an error if the nested process does not return true.
    /// </summary>
    public sealed class AssertTrueProcessFactory : SimpleRunnableProcessFactory<AssertTrue, Unit>
    {
        private AssertTrueProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<AssertTrue, Unit> Instance { get; } = new AssertTrueProcessFactory();
    }

    /// <summary>
    /// Returns an error if the nested process does not return true.
    /// </summary>
    public sealed class AssertTrue : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState) =>
            Test.Run(processState).Ensure(x => x,
                new RunError($"Assertion Failed '{Test.Name}'", Name, null, ErrorCode.IndexOutOfBounds)).Map(x=> Unit.Default);

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => AssertTrueProcessFactory.Instance;

        /// <summary>
        /// The process to test.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Test { get; set; } = null!;
    }
}
