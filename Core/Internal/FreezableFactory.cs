using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Methods to create freezable types
    /// </summary>
    public static class FreezableFactory
    {
        //TODO move other CreateFreezable methods here


        /// <summary>
        /// Create a new Freezable Array
        /// </summary>
        public static IFreezableStep CreateFreezableList(IReadOnlyCollection<IFreezableStep> elements, Configuration? configuration, IErrorLocation location)
        {
            if (elements.All(x => x is CreateEntityFreezableStep || x is ConstantFreezableStep cfs && cfs.Value.IsT6))
            {
                var dict = new Dictionary<string, FreezableStepProperty>
                {
                    {nameof(EntityStreamCreate.Elements), new FreezableStepProperty(elements.ToList(), location)}
                };

                var fpd = new FreezableStepData(dict, location);

                return new CompoundFreezableStep(EntityStreamCreateStepFactory.Instance.TypeName, fpd, configuration);
            }
            else
            {
                var dict = new Dictionary<string, FreezableStepProperty>
                {
                    {nameof(Array<object>.Elements), new FreezableStepProperty(elements.ToList(), location)}
                };

                var fpd = new FreezableStepData(dict, location);

                return new CompoundFreezableStep(ArrayStepFactory.Instance.TypeName, fpd, configuration);
            }
        }

    }
}
