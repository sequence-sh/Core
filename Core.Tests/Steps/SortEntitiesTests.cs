using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SortEntitiesTests : StepTestBase<SortEntities, EntityStream >
    {
        /// <inheritdoc />
        public SortEntitiesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield return new StepCase("Ascending",
                    new ForEachEntity
                    {
                        EntityStream = new SortEntities
                        {
                            SortAscending = Constant(true),
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Gamma")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Beta"))
                            ))
                        }

                    }, Unit.Default,

                    "Alpha", "Beta", "Gamma"
                )
            ;}
        }

    }
}