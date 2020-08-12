using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses
{
    /// <summary>
    /// A runnable process that is not a constant or a reference to a variable.
    /// </summary>
    public abstract class CompoundRunnableProcess<T> : IRunnableProcess<T>
    {
        /// <inheritdoc />
        public abstract Result<T> Run(ProcessState processState);

        /// <summary>
        /// The factory used to create processes of this type.
        /// </summary>
        public abstract RunnableProcessFactory RunnableProcessFactory { get; }

        /// <inheritdoc />
        public string Name => RunnableProcessFactory.ProcessNameBuilder.GetFromArguments(FreezableProcessData);

        /// <inheritdoc />
        public override string ToString() => RunnableProcessFactory.TypeName;



        private FreezableProcessData FreezableProcessData
        {
            get
            {
                var variableNames = GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .Select(x => (x.Name, variableName: (VariableName)x.GetValue(this)))
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
        public IFreezableProcess Unfreeze() => new CompoundFreezableProcess(RunnableProcessFactory,FreezableProcessData);
    }
}