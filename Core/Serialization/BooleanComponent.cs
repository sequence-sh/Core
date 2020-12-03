//using CSharpFunctionalExtensions;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Util;

//namespace Reductech.EDR.Core.Serialization
//{
//    /// <summary>
//    /// Deserializes a regex group into a bool.
//    /// </summary>
//    public class BooleanComponent : ISerializerBlock, IStepSerializerComponent
//    {
//        /// <summary>
//        /// Creates a new BooleanComponent
//        /// </summary>
//        public BooleanComponent(string propertyName) => PropertyName = propertyName;

//        /// <summary>
//        /// The property name
//        /// </summary>
//        public string PropertyName { get; }

//        /// <inheritdoc />
//        public Result<string> TryGetText(FreezableStepData data, StepFactoryStore stepFactoryStore) =>
//            data.InitialSteps
//                .TryFindOrFail(PropertyName, null)
//                .Bind(x => x.Match(VariableNameComponent.SerializeAsync,
//                    step=> TrySerialize(step, stepFactoryStore),
//                    _ => Result.Failure<string>("Cannot serialize list")

//                ));

//        private static Result<string> TrySerialize(IFreezableStep step, StepFactoryStore stepFactoryStore)
//        {
//            if (step is ConstantFreezableStep constantFreezableProcess && constantFreezableProcess.Value.Value is bool b)
//                return b.ToString();
//            if (step is CompoundFreezableStep compound && compound.StepConfiguration == null)
//            {
//                return compound.TryGetStepFactory(stepFactoryStore)
//                    .MapError1(x => x.AsString)
//                    .Bind(x =>
//                        x.Serializer.TrySerialize(compound.FreezableStepData, stepFactoryStore));
//            }


//            return Result.Failure<string>("Cannot serialize compound as a primitive");
//        }

//        /// <inheritdoc />
//        public ISerializerBlock SerializerBlock => this;
//    }
//}