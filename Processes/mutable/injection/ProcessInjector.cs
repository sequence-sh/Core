using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.mutable.injection
{
    internal interface IProcessInjector
    {
        Result Inject(Process process);
    }

    internal class ProcessInjector : IProcessInjector
    {
        private readonly List<(string element, Injection injection)> _injections;

        public void Add(string element, Injection injection)
        {
            _injections.Add((element, injection));
        }

        public ProcessInjector()
        {
            _injections = new List<(string element, Injection injection)>();
        }

        public ProcessInjector(IEnumerable<(string element, Injection injection)> injections)
        {
            _injections = injections.ToList();
        }

        public Result Inject(Process process)
        {
            foreach (var (element, injection) in _injections)
            {
                var pathResult = InjectionParser.TryParse(injection.Property);

                if (pathResult.IsFailure)
                    return pathResult;

                var propertyValueResult = injection.GetPropertyValue(element);
                if (propertyValueResult.IsFailure)
                    return propertyValueResult;
                
                var setResult = pathResult.Value.TrySetValue(process, propertyValueResult.Value);

                if (setResult.IsFailure)
                    return setResult;
            }

            return Result.Success();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ProcessInjector pi &&
                   _injections.Select(x => (x.element, x.injection.Property))
                       .OrderBy(x=>x.Property)
                       .SequenceEqual(
                           pi._injections.Select(x => (x.element, x.injection.Property))
                           .OrderBy(x=>x.Property));
        }


        /// <inheritdoc />
        public override int GetHashCode()
        {
            //TODO improve this hash code
            var s = string.Join("", _injections.Select(i => i.element + i.injection.Property));

            return s.GetHashCode();
        }
    }
}