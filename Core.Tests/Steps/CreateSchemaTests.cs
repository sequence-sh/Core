using System.Collections.Generic;
using System.Collections.Immutable;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class CreateSchemaTests : StepTestBase<CreateSchema, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Create Schema",
                new CreateSchema()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities   = Array(Entity.Create(("StringProp1", "abc"), ("IntProp", 123)))
                },
                new Schema()
                {
                    Name = "Test Schema",
                    Properties =
                        new Dictionary<string, SchemaProperty>()
                        {
                            {
                                "StringProp1",
                                new SchemaProperty()
                                {
                                    Type         = SCLType.String,
                                    Multiplicity = Multiplicity.ExactlyOne
                                }
                            },
                            {
                                "IntProp",
                                new SchemaProperty()
                                {
                                    Type         = SCLType.Integer,
                                    Multiplicity = Multiplicity.ExactlyOne
                                }
                            },
                        }.ToImmutableSortedDictionary(),
                    ExtraProperties = ExtraPropertyBehavior.Fail
                }.ConvertToEntity()
            );
        }
    }
}

}
