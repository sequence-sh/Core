using System;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{
    /// <summary>
    /// Makes sure all steps have a test class
    /// </summary>
    public class MetaTests
    {
        [Fact]
        public void All_steps_should_have_a_step_test()
        {
            var stepTypes = typeof(ICompoundStep).Assembly.GetTypes().Where(x => typeof(ICompoundStep).IsAssignableFrom(x) && !x.IsAbstract).ToHashSet();

            var testedStepTypes = typeof(MetaTests).Assembly.GetTypes()
                .Where(x => typeof(IStepTestBase).IsAssignableFrom(x) && !x.IsAbstract).Select(GetStepType).ToHashSet();


            var untestedSteps = stepTypes.Except(testedStepTypes);

            untestedSteps.Should().BeEmpty();
        }

        private static Type GetStepType(Type testType)
        {
            var constructor = testType.GetConstructors().First();

            var parameters = constructor.GetParameters().Select(x => x.DefaultValue ?? null).Select(x=> x == DBNull.Value? null : x).ToArray();

            var instance = constructor.Invoke(parameters);

            var stepTestBase = (IStepTestBase) instance;

            if(stepTestBase.StepType.IsGenericType)
                return stepTestBase.StepType.GetGenericTypeDefinition();

            return stepTestBase.StepType;
        }

    }
}
