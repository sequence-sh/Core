using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.Mutable.Injections
{
    /// <summary>
    /// Injects values into a process.
    /// </summary>
    public interface IProcessInjector
    {
        /// <summary>
        /// Inject the values into the process.
        /// </summary>
        Result Inject(Process process);
    }

    /// <summary>
    /// Injects values into a process.
    /// </summary>
    public class ProcessInjector : IProcessInjector
    {
        private readonly List<(object element, Injection injection)> _injections;


        /// <summary>
        /// Add a new injection.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="injection"></param>
        public void Add(object element, Injection injection)
        {
            _injections.Add((element, injection));
        }

        /// <summary>
        /// Create a new ProcessInjector.
        /// </summary>
        public ProcessInjector()
        {
            _injections = new List<(object element, Injection injection)>();
        }

        /// <summary>
        /// Create a new ProcessInjector.
        /// </summary>
        public ProcessInjector(IEnumerable<(object element, Injection injection)> injections)
        {
            _injections = injections.ToList();
        }

        /// <summary>
        /// Is this injection valid?
        /// </summary>
        public bool IsValid => _injections.All(x => x.injection.GetPropertyValue(x.element).IsSuccess);

        /// <inheritdoc />
        public Result Inject(Process process)
        {
            foreach (var (element, injection) in _injections)
            {
                var setResult = injection.TryInject(element, process);

                if (setResult.IsFailure)
                    return setResult;
            }

            return Result.Success();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ProcessInjector pi &&
                   _injections.Select(x => (x.injection.GetPropertyValue(x.element), x.injection.Property))
                       .OrderBy(x=>x.Property)
                       .SequenceEqual(
                           pi._injections.Select(x => (x.injection.GetPropertyValue(x.element), x.injection.Property))
                           .OrderBy(x=>x.Property));
        }


        /// <inheritdoc />
        public override int GetHashCode()
        {
            //TODO improve this hash code
            var s = string.Join("", _injections.Select(i => i.injection.GetPropertyValue(i.element) + i.injection.Property));

            return s.GetHashCode();
        }
    }
}