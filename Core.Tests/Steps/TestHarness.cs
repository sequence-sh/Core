using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy.Internal;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using Namotion.Reflection;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.Tests.Steps
{
    public abstract class StepTestBase<TStep, TOutput> where TStep : ICompoundStep<TOutput>, new()
    {
        protected StepTestBase(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        public string StepName => typeof(TStep).GetDisplayName();

        //TODO serialization

        //TODO Deserialization


        public ITestOutputHelper TestOutputHelper {get;}


        [Fact]
        public void All_properties_should_be_required_or_have_default_values_and_attributes()
        {
            var instance = CreateInstance();
            var errors = new List<string>();

            foreach (var propertyInfo in typeof(TStep).GetProperties().Where(x=>x.GetCustomAttribute<StepPropertyBaseAttribute>() != null))
            {
                var propName = $"{typeof(TStep).GetDisplayName()}.{propertyInfo.Name}";


                var required = propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;
                var hasDefaultAttribute = propertyInfo.GetCustomAttribute<DefaultValueExplanationAttribute>() != null;

                if (propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic)
                    errors.Add($"{propName} has no public setter");

                if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
                {
                    errors.Add($"{propName} has no public getter");
                    continue;
                }

                var defaultValue = propertyInfo.GetValue(instance);

                if (required)
                {
                    if(hasDefaultAttribute)
                        errors.Add($"{propName} has both required and defaultValueExplanation attributes");

                    if(defaultValue != null)
                        errors.Add($"{propName} is required but it has a default value");
                }
                else if(hasDefaultAttribute)
                {
                    if(!propertyInfo.PropertyType.IsNullableType() && defaultValue == null)
                        errors.Add($"{propName} has a default value explanation but is not nullable and it's default value is null");
                }
                else
                {
                    errors.Add($"{propName} has neither required nor defaultValueExplanation attributes");
                }
            }

            if(errors.Any())
                throw new XunitException(string.Join("\r\n", errors));

        }


        [Fact]
        public void Process_factory_must_be_set_correctly()
        {
            var instance = CreateInstance();

            instance.StepFactory.Should().NotBeNull();

            instance.StepFactory.StepType.Should().Be(typeof(TStep));

            var stepFactoryType = instance.StepFactory.GetType();

            var constructor = stepFactoryType.GetConstructor(new Type[] { });
            constructor.Should().BeNull($"{StepName} should not have a public parameterless constructor");

            var instanceProperty = stepFactoryType.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);

            instanceProperty.Should().NotBeNull($"{StepName} should have a public static Get property named 'Instance'");
            instanceProperty.Should().NotBeWritable($"{StepName}.Instance should be readonly");

            instanceProperty!.PropertyType.Should().BeAssignableTo<IStepFactory>($"{StepName}.Instance should return an IStepFactory");
        }



        protected TStep CreateInstance() => Activator.CreateInstance<TStep>();


        protected abstract IEnumerable<StepCase> StepCases { get; }

        public IEnumerable<object?[]> StepCaseNames => StepCases.Select(x => new[]{x.Name});


        [Theory]
        [NonStaticMemberData(nameof(StepCaseNames))]
        public async Task Should_behave_as_expected_when_run(string stepCaseName)
        {
            TestOutputHelper.WriteLine(stepCaseName);

            var stepCase = StepCases.Single(x => x.Name == stepCaseName);

            TestOutputHelper.WriteLine(stepCase.Step.Name);

            var logger = new TestLogger();

            var factory = new MockRepository(MockBehavior.Strict);
            var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();

            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

            stepCase.SetupMockExternalProcessRunner?.Invoke(externalProcessRunnerMock);
            var stateMonad = new StateMonad(logger, EmptySettings.Instance, externalProcessRunnerMock.Object, sfs);

            var output = await stepCase.Step.Run<TOutput>(stateMonad, CancellationToken.None);

            output.ShouldBeSuccessful(x => x.AsString);

            output.Value.Should().Be(stepCase.ExpectedOutput);

            logger.LoggedValues.Should().BeEquivalentTo(stepCase.ExpectedLoggedValues);


            factory.VerifyAll();
        }

        [Theory]
        [NonStaticMemberData(nameof(StepCaseNames))]
        public async Task Should_behave_as_expected_when_serialized_deserialized_and_executed(string stepCaseName)
        {
            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

            TestOutputHelper.WriteLine(stepCaseName);

            var stepCase = StepCases.Single(x => x.Name == stepCaseName);

            var yaml = stepCase.Step.Unfreeze().SerializeToYaml();

            TestOutputHelper.WriteLine(yaml);


            var deserializeResult = YamlMethods.DeserializeFromYaml(yaml, sfs);

            deserializeResult.ShouldBeSuccessful(x=>x.AsString);

            var freezeResult = deserializeResult.Value.TryFreeze();
            freezeResult.ShouldBeSuccessful(x=>x.AsString);

            var logger = new TestLogger();

            var factory = new MockRepository(MockBehavior.Strict);
            var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();


            stepCase.SetupMockExternalProcessRunner?.Invoke(externalProcessRunnerMock);

            var stateMonad = new StateMonad(logger, EmptySettings.Instance, externalProcessRunnerMock.Object, sfs);

            var output = await freezeResult.Value.Run<TOutput>(stateMonad, CancellationToken.None);

            output.ShouldBeSuccessful(x=>x.AsString);

            output.Value.Should().Be(stepCase.ExpectedOutput);

            logger.LoggedValues.Should().BeEquivalentTo(stepCase.ExpectedLoggedValues);


            factory.VerifyAll();
        }


        protected abstract IEnumerable<DeserializeCase> DeserializeCases { get; }

        public IEnumerable<object?[]> DeserializeCaseNames => DeserializeCases.Select(x => new[] { x.Name });

        [Theory]
        [NonStaticMemberData(nameof(DeserializeCaseNames))]
        public async Task Should_behave_as_expected_when_deserialized(string deserializeCaseName)
        {
            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

            TestOutputHelper.WriteLine(deserializeCaseName);

            var deserializeCase = DeserializeCases.Single(x => x.Name == deserializeCaseName);

            var yaml = deserializeCase.Yaml;

            TestOutputHelper.WriteLine(yaml);


            var deserializeResult = YamlMethods.DeserializeFromYaml(yaml, sfs);

            deserializeResult.ShouldBeSuccessful(x => x.AsString);

            var freezeResult = deserializeResult.Value.TryFreeze();
            freezeResult.ShouldBeSuccessful(x => x.AsString);

            var logger = new TestLogger();

            var factory = new MockRepository(MockBehavior.Strict);
            var externalProcessRunnerMock = factory.Create<IExternalProcessRunner>();


            deserializeCase.SetupMockExternalProcessRunner?.Invoke(externalProcessRunnerMock);

            var stateMonad = new StateMonad(logger, EmptySettings.Instance, externalProcessRunnerMock.Object, sfs);

            var output = await freezeResult.Value.Run<TOutput>(stateMonad, CancellationToken.None);

            output.ShouldBeSuccessful(x => x.AsString);

            output.Value.Should().Be(deserializeCase.ExpectedOutput);

            logger.LoggedValues.Should().BeEquivalentTo(deserializeCase.ExpectedLoggedValues);


            factory.VerifyAll();
        }


        protected abstract IEnumerable<SerializeCase> SerializeCases { get; }

        public IEnumerable<object?[]> SerializeCaseNames => SerializeCases.Select(x => new[] { x.Name });

        [Theory]
        [NonStaticMemberData(nameof(SerializeCaseNames))]
        public void Should_serialize_as_expected(string serializeCaseName)
        {
            TestOutputHelper.WriteLine(serializeCaseName);

            var serializeCase = SerializeCases.Single(x => x.Name == serializeCaseName);

            var realYaml = serializeCase.Step.Unfreeze().SerializeToYaml();

            realYaml.Should().Be(serializeCase.ExpectedYaml);
        }

        public class SerializeCase
        {
            public SerializeCase(string name, TStep step, string expectedYaml, Configuration? expectedConfiguration = null)
            {
                Name = name;
                Step = step;
                ExpectedYaml = expectedYaml;
                ExpectedConfiguration = expectedConfiguration;
            }

            public string Name { get; }
            public TStep Step { get; }
            public string ExpectedYaml { get; }

            public Configuration? ExpectedConfiguration { get; }

        }

        public class DeserializeCase : StepCaseBase
        {
            /// <inheritdoc />
            public DeserializeCase(string name, string yaml, TOutput expectedOutput, params object[] expectedLoggedValues) : base(name, expectedOutput, expectedLoggedValues) => Yaml = yaml;

            public string Yaml { get; }
        }

        public abstract class StepCaseBase
        {
            protected StepCaseBase(string name, TOutput expectedOutput, object[] expectedLoggedValues)
            {
                Name = name;
                ExpectedOutput = expectedOutput;
                ExpectedLoggedValues = expectedLoggedValues;
            }

            public string Name { get; }

            public TOutput ExpectedOutput { get; }

            public object[] ExpectedLoggedValues { get; }

            public Action<Mock<IExternalProcessRunner>>? SetupMockExternalProcessRunner { get; set; }

            /// <inheritdoc />
            public override string ToString() => Name;
        }


        public class StepCase : StepCaseBase
        {
            public StepCase(string name, TStep step, TOutput expectedOutput, params object[] expectedLoggedValues) : base(name, expectedOutput, expectedLoggedValues) => Step = step;

            public TStep Step { get; }
        }

    }


    public class NotTests : StepTestBase<Not, bool>
    {
        /// <inheritdoc />
        public NotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Not True", new Not { Boolean = new Constant<bool>(true) }, false);
                yield return new StepCase("Not False", new Not { Boolean = new Constant<bool>(false) }, true);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases {
            get
            {
                yield return new DeserializeCase("Short Form", "not (true)", false);
                yield return new DeserializeCase("Long Form", "do: not\nboolean: true", false);
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                yield return new SerializeCase("Serialize", new Not() {Boolean = new Constant<bool>(true)},
                    "not(True)");
            } }
    }
}
