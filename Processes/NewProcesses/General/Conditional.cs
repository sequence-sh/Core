using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class Conditional : CompoundRunnableProcess<Unit>
    {


        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            var result = Condition.Run(processState)
                .Bind(r =>
                {
                    if (r)
                        return ThenProcess.Run(processState);
                    return ElseProcess?.Run(processState) ?? Result.Success(Unit.Default);
                });

            return result;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ConditionalProcessFactory.Instance;

        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<Unit> ThenProcess { get; set; } = null!;

        //TODO else if
        //public IReadOnlyList<IRunnableProcess<Unit>> ElseIfProcesses

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<Unit>? ElseProcess { get; set; } = null;

    }


    public sealed class ConditionalProcessFactory : RunnableProcessFactory
    {
        private ConditionalProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new ConditionalProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override string TypeName => nameof(Conditional);

        /// <inheritdoc />
        public override string GetProcessName(IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var conditionName = processArguments.TryFind(nameof(Conditional.Condition)).Map(x => x.ProcessName)
                .Unwrap("Condition");
            var thenName = processArguments.TryFind(nameof(Conditional.ThenProcess)).Map(x => x.ProcessName).Unwrap("??");
            var elseName = processArguments.TryFind(nameof(Conditional.ElseProcess)).Map(x => x.ProcessName).Unwrap(null);


            return ProcessNameHelper.GetConditionalName(conditionName, thenName, elseName);
        }

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) => new Conditional();

    }
}
