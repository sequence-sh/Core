namespace Sequence.Core.Tests.Steps;

public partial class
    EntityGetPropertiesTests : StepTestBase<EntityGetProperties, Array<StringStream>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Get Properties",
                new EntityGetProperties()
                {
                    Entity = StaticHelpers.Constant(Entity.Create(("a", 1), ("b", 2)))
                },
                new[] { "a", "b" }.Select(x => new StringStream(x)).ToSCLArray()
            );
        }
    }
}
