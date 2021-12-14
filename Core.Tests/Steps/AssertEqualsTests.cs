namespace Reductech.EDR.Core.Tests.Steps;

public partial class AssertEqualTests : StepTestBase<AssertEqual<StringStream>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Strings equal",
                new AssertEqual<StringStream>
                {
                    Left = Constant("Hello"), Right = Constant("Hello")
                },
                Unit.Default
            );

            yield return new StepCase(
                "Ints equal",
                new AssertEqual<SCLInt> { Left = Constant(2), Right = Constant(2) },
                Unit.Default
            );

            yield return new StepCase(
                "Entities equal",
                new AssertEqual<Entity>
                {
                    Left  = Constant(Entity.Create(("a", 1))),
                    Right = Constant(Entity.Create(("a", 1)))
                },
                Unit.Default
            );

            yield return new StepCase(
                "Compare to entity int property",
                new AssertEqual<SCLInt>
                {
                    Left = Constant(1),
                    Right = new EntityGetValue<SCLInt>
                    {
                        Entity   = Constant(Entity.Create(("Foo", 1))),
                        Property = Constant("Foo")
                    }
                },
                Unit.Default
            );

            yield return new StepCase(
                "Compare to entity property from variable",
                new Sequence<Unit>()
                {
                    InitialSteps =
                        new[]
                        {
                            new SetVariable<Entity>()
                            {
                                Variable = new VariableName("MyEntity"),
                                Value    = Constant(Entity.Create(("Foo", 1)))
                            }
                        },
                    FinalStep = new AssertEqual<SCLInt>
                    {
                        Left = Constant(1),
                        Right = new EntityGetValue<SCLInt>
                        {
                            Entity   = GetVariable<Entity>("MyEntity"),
                            Property = Constant("Foo")
                        }
                    }
                },
                Unit.Default
            ) { IgnoreFinalState = true };

            yield return new StepCase(
                "Strings not equal",
                new AssertError()
                {
                    Step = new AssertEqual<StringStream>
                    {
                        Left = Constant("Hello"), Right = Constant("World")
                    }
                },
                Unit.Default
            ) { IgnoreLoggedValues = true };

            yield return new StepCase(
                "Ints not equal",
                new AssertError()
                {
                    Step = new AssertEqual<SCLInt> { Left = Constant(2), Right = Constant(3) }
                },
                Unit.Default
            ) { IgnoreLoggedValues = true };

            yield return new StepCase(
                "Entities not equal",
                new AssertError()
                {
                    Step = new AssertEqual<Entity>
                    {
                        Left  = Constant(Entity.Create(("a", 1))),
                        Right = Constant(Entity.Create(("b", 1)))
                    }
                },
                Unit.Default
            ) { IgnoreLoggedValues = true };
        }
    }
}
