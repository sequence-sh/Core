using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A runnable step that is not a constant.
    /// </summary>
    public abstract class CompoundStep<T> : ICompoundStep<T>
    {
        /// <inheritdoc />
        public abstract Result<T, IRunErrors> Run(StateMonad stateMonad);

        /// <summary>
        /// The factory used to create steps of this type.
        /// </summary>
        public abstract IStepFactory StepFactory { get; }

        /// <inheritdoc />
        public string Name => StepFactory.StepNameBuilder.GetFromArguments(FreezableStepData);

        /// <inheritdoc />
        public override string ToString() => StepFactory.TypeName;

        /// <summary>
        /// Configuration for this step.
        /// </summary>
        public Configuration? Configuration { get; set; }

        /// <inheritdoc />
        public virtual IEnumerable<Requirement> RuntimeRequirements => ImmutableArray<Requirement>.Empty;

        /// <inheritdoc />
        public IEnumerable<IStepCombiner> StepCombiners =>
            StepFactory.StepCombiner.ToList()
                .Concat(
                    RunnableArguments.Select(x => x.step)
                        .Concat(RunnableListArguments.SelectMany(x => x.list))
                        .OfType<ICompoundStep>()
                        .SelectMany(x => x.StepCombiners)

                ).Distinct();


        private IEnumerable<(string name, IStep step) > RunnableArguments
        {
            get
            {
                return GetType().GetProperties()
                    .Where(x => x.GetCustomAttribute<StepPropertyAttribute>() != null)
                    .Select(x => (x.Name, step: x.GetValue(this) as IStep))
                    .Where(x => x.step != null)!;
            }
        }

        private IEnumerable<(string name, IEnumerable<IStep> list)> RunnableListArguments
        {
            get
            {
                return GetType()
                    .GetProperties()
                    .Where(x => x.GetCustomAttribute<StepListPropertyAttribute>() != null)
                    .Select(x => (x.Name, list: x.GetValue(this) as IEnumerable<IStep>))
                    .Where(x => x.list != null)!;
            }
        }

        private FreezableStepData FreezableStepData
        {
            get
            {
                var variableNames = GetType().GetProperties()
                .Where(x => x.GetCustomAttribute<VariableNameAttribute>() != null)
                .Select(x => (x.Name, variableName: (VariableName)x.GetValue(this)!))
                .Where(x => x.variableName != null)

                .ToDictionary(x => x.Name, x => new StepMember(x.variableName)  );


                var arguments  = RunnableArguments
                 .ToDictionary(x => x.name, x => new StepMember(x.step!.Unfreeze()));

                var listArguments = RunnableListArguments
                .ToDictionary(x => x.name,
                    x => new StepMember( x.list.Select(y => y.Unfreeze()).ToList()));


                var stepMembers = variableNames.Concat(arguments).Concat(listArguments)
                    .ToDictionary(x => x.Key, x => x.Value);


                return new FreezableStepData(stepMembers);
            }
        }

        /// <inheritdoc />
        public IFreezableStep Unfreeze() => new CompoundFreezableStep(StepFactory,FreezableStepData, Configuration);

        /// <inheritdoc />
        public Result<T1, IRunErrors> Run<T1>(StateMonad stateMonad) =>
            Run(stateMonad).BindCast<T, T1, IRunErrors>(new RunError($"Could not cast {typeof(T)} to {typeof(T1)}", Name, null, ErrorCode.InvalidCast));

        /// <summary>
        /// Check that this step meets requirements
        /// </summary>
        public virtual Result<Unit, IRunErrors> VerifyThis(ISettings settings) => Unit.Default;


        /// <inheritdoc />
        public Result<Unit, IRunErrors> Verify(ISettings settings)
        {
            var r0 = new[] {VerifyThis(settings)};

            var rRequirements = RuntimeRequirements.Concat(StepFactory.Requirements)
                .Select(req => settings.CheckRequirement(Name, req));


            var r1 = RunnableArguments.Select(x => x.step.Verify(settings));
            var r2 = RunnableListArguments.Select(x => x.list.Select(l => l.Verify(settings))
                    .Combine(RunErrorList.Combine).Map(_=>Unit.Default));


            var finalResult = r0.Concat(rRequirements) .Concat(r1).Concat(r2).Combine(RunErrorList.Combine).Map(_ => Unit.Default);

            return finalResult;
        }
    }
}