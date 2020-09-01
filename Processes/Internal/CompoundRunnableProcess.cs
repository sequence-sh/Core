using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public abstract class CompoundRunnableProcess<T> : ICompoundRunnableProcess<T>
    {
        /// <inheritdoc />
        public abstract Result<T, IRunErrors> Run(ProcessState processState);

        /// <summary>
        /// The factory used to create processes of this type.
        /// </summary>
        public abstract IRunnableProcessFactory RunnableProcessFactory { get; }

        /// <inheritdoc />
        public string Name => RunnableProcessFactory.ProcessNameBuilder.GetFromArguments(FreezableProcessData);

        /// <inheritdoc />
        public override string ToString() => RunnableProcessFactory.TypeName;

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        public ProcessConfiguration? ProcessConfiguration { get; set; }

        /// <inheritdoc />
        public virtual IEnumerable<Requirement> RuntimeRequirements => ImmutableArray<Requirement>.Empty;


        private IEnumerable<(string name, IRunnableProcess process) > RunnableArguments
        {
            get
            {
                return GetType().GetProperties()
                    .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                    .Select(x => (x.Name, process: x.GetValue(this) as IRunnableProcess))
                    .Where(x => x.process != null)!;
            }
        }

        private IEnumerable<(string name, IEnumerable<IRunnableProcess> list)> RunnableListArguments
        {
            get
            {
                return GetType()
                    .GetProperties()
                    .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                    .Select(x => (x.Name, list: x.GetValue(this) as IEnumerable<IRunnableProcess>))
                    .Where(x => x.list != null)!;
            }
        }

        private FreezableProcessData FreezableProcessData
        {
            get
            {
                var variableNames = GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .Select(x => (x.Name, variableName: (VariableName)x.GetValue(this)!))
                .Where(x => x.variableName != null)

                .ToDictionary(x => x.Name, x => new ProcessMember(x.variableName)  );


                var arguments  = RunnableArguments
                 .ToDictionary(x => x.name, x => new ProcessMember(x.process!.Unfreeze()));

                var listArguments = RunnableListArguments
                .ToDictionary(x => x.name,
                    x => new ProcessMember( x.list.Select(y => y.Unfreeze()).ToList()));


                var processMembers = variableNames.Concat(arguments).Concat(listArguments)
                    .ToDictionary(x => x.Key, x => x.Value);


                return new FreezableProcessData(processMembers);
            }
        }

        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new CompoundFreezableProcess(RunnableProcessFactory,FreezableProcessData, ProcessConfiguration);

        /// <inheritdoc />
        public Result<T1, IRunErrors> Run<T1>(ProcessState processState) =>
            Run(processState).BindCast<T, T1, IRunErrors>(new RunError($"Could not cast {typeof(T)} to {typeof(T1)}", Name, null, ErrorCode.InvalidCast));

        /// <summary>
        /// Check that this process meets requirements
        /// </summary>
        public virtual Result<Unit, IRunErrors> VerifyThis => Unit.Default;


        /// <inheritdoc />
        public Result<Unit, IRunErrors> Verify(IProcessSettings settings)
        {
            var r0 = new[] {VerifyThis};

            var rRequirements = RuntimeRequirements.Concat(RunnableProcessFactory.Requirements)
                .Select(req => settings.CheckRequirement(Name, req));


            var r1 = RunnableArguments.Select(x => x.process.Verify(settings));
            var r2 = RunnableListArguments.Select(x => x.list.Select(l => l.Verify(settings))
                    .Combine(RunErrorList.Combine).Map(_=>Unit.Default));


            var finalResult = r0.Concat(rRequirements) .Concat(r1).Concat(r2).Combine(RunErrorList.Combine).Map(_ => Unit.Default);

            return finalResult;
        }
    }
}