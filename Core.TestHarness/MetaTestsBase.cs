namespace Reductech.EDR.Core.TestHarness;

public abstract class MetaTestsBase
{
    /// <summary>
    /// The assembly containing steps to test.
    /// </summary>
    public abstract Assembly StepAssembly { get; }

    /// <summary>
    /// The assembly containing tests.
    /// </summary>
    public abstract Assembly TestAssembly { get; }

    [Fact]
    public void All_steps_should_have_a_step_test()
    {
        var stepTypes = StepAssembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(ICompoundStep).IsAssignableFrom(x))
            .ToHashSet();

        var testedStepTypes = TestAssembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(IStepTestBase).IsAssignableFrom(x))
            .Select(GetStepType)
            .ToHashSet();

        var untestedSteps = stepTypes.Except(testedStepTypes);

        untestedSteps.Should().BeEmpty();
    }

    private static Type GetStepType(Type testType)
    {
        var constructor = testType.GetConstructors().First();

        var parameters = constructor.GetParameters()
            .Select(x => x.DefaultValue ?? null)
            .Select(x => x == DBNull.Value ? null : x)
            .ToArray();

        var instance = constructor.Invoke(parameters);

        var stepTestBase = (IStepTestBase)instance;

        if (stepTestBase.StepType.IsGenericType)
            return stepTestBase.StepType.GetGenericTypeDefinition();

        return stepTestBase.StepType;
    }
}
