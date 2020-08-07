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
        public string Name => RunnableProcessFactory.GetProcessName(Arguments, ListArguments);

        /// <inheritdoc />
        public override string ToString() => RunnableProcessFactory.TypeName;


        private IReadOnlyDictionary<string, IFreezableProcess> Arguments
        {
            get
            {
                var result = GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessPropertyAttribute>() != null)
                .Select(x => (x.Name, process: x.GetValue(this) as IRunnableProcess))
                .Where(x => x.process != null)
                .ToDictionary(x => x.Name, x => x.process!.Unfreeze());

                return result;
            }
        }


        private IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> ListArguments
        {
            get
            {
                var result = GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttribute<RunnableProcessListPropertyAttribute>() != null)
                .Select(x => (x.Name, list: x.GetValue(this) as IEnumerable<IRunnableProcess>))
                .Where(x => x.list != null)
                .ToDictionary(x => x.Name,
                    x => x.list.Select(y => y.Unfreeze()).ToList() as IReadOnlyList<IFreezableProcess>);

                return result;
            }
        }

        /// <inheritdoc />
        public IFreezableProcess Unfreeze() => new CompoundFreezableProcess(RunnableProcessFactory,Arguments, ListArguments);
    }
}