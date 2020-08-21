using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public interface ICompoundRunnableProcess : IRunnableProcess
    {
        /// <summary>
        /// The factory used to create processes of this type.
        /// </summary>
        IRunnableProcessFactory RunnableProcessFactory { get; }

        /// <summary>
        /// Configuration for this process.
        /// </summary>
        ProcessConfiguration? ProcessConfiguration { get; set; }
    }

    /// <summary>
    /// A runnable process that is not a constant.
    /// </summary>
    public abstract class CompoundRunnableProcess<T> : IRunnableProcess<T>, ICompoundRunnableProcess
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


        private FreezableProcessData FreezableProcessData
        {
            get
            {
                var variableNames = GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .Select(x => (x.Name, variableName: (VariableName)x.GetValue(this)!))
                .Where(x => x.variableName != null)

                .ToDictionary(x => x.Name, x => new ProcessMember(x.variableName)  );


                var arguments  = GetType().GetProperties()
                 .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                 .Select(x => (x.Name, process: x.GetValue(this) as IRunnableProcess))
                 .Where(x => x.process != null)
                 .ToDictionary(x => x.Name, x => new ProcessMember(x.process!.Unfreeze()));

                var listArguments = GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                .Select(x => (x.Name, list: x.GetValue(this) as IEnumerable<IRunnableProcess>))
                .Where(x => x.list != null)
                .ToDictionary(x => x.Name,
                    x => new ProcessMember( x.list.Select(y => y.Unfreeze()).ToList()));


                var processMembers = variableNames.Concat(arguments).Concat(listArguments)
                    .ToDictionary(x => x.Key, x => x.Value);


                return new FreezableProcessData(processMembers);
            }
        }

        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new CompoundFreezableProcess(RunnableProcessFactory,FreezableProcessData, ProcessConfiguration);

        /// <inheritdoc />
        public Result<T1, IRunErrors> Run<T1>(ProcessState processState)
        {


            return Run(processState).BindCast<T, T1, IRunErrors>(new RunError($"Could not cast {typeof(T)} to {typeof(T1)}", Name, null, ErrorCode.InvalidCast));// .Bind(x => x.TryCast<T1>());
        }
    }
}