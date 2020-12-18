using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A runnable step that is not a constant.
    /// </summary>
    public abstract class CompoundStep<T> : ICompoundStep<T>
    {
        /// <inheritdoc />
        public abstract Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken);


        /// <inheritdoc />
        public Task<Result<T1, IError>> Run<T1>(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            return Run(stateMonad, cancellationToken).BindCast<T, T1, IError>(
                    new SingleError($"Could not cast {typeof(T)} to {typeof(T1)}", ErrorCode.InvalidCast, new StepErrorLocation(this)));
        }

        /// <summary>
        /// The factory used to create steps of this type.
        /// </summary>
        public abstract IStepFactory StepFactory { get; }

        /// <inheritdoc />
        public string Serialize() => StepFactory.Serializer.Serialize(AllProperties);

        /// <inheritdoc />
        public string Name => StepFactory.TypeName;

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// Configuration for this step.
        /// </summary>
        public Configuration? Configuration { get; set; }

        /// <inheritdoc />
        public virtual IEnumerable<Requirement> RuntimeRequirements => ImmutableArray<Requirement>.Empty;

        /// <inheritdoc />
        public virtual bool ShouldBracketWhenSerialized => true;

        /// <inheritdoc />
        public Type OutputType => typeof(T);

        /// <summary>
        /// All properties of this step
        /// </summary>
        public IEnumerable<StepProperty> AllProperties
        {
            get
            {
                var r = GetType().GetProperties()
                    .Select(propertyInfo => (propertyInfo,
                        attribute: propertyInfo.GetCustomAttribute<StepPropertyBaseAttribute>()))
                    .Where(x => x.attribute != null)
                    .OrderBy(x => x.attribute!.Order)
                    .SelectMany((x,i)=> GetMember(x, i).ToEnumerable());


                return r;

                Maybe<StepProperty> GetMember((PropertyInfo propertyInfo, StepPropertyBaseAttribute? attribute) arg1, int arg2)
                {
                    var (propertyInfo, _) = arg1;
                    var val = propertyInfo.GetValue(this);

                    return val switch
                    {
                        IStep step => new StepProperty(propertyInfo.Name, arg2,
                            OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(step)),
                        IEnumerable<IStep> enumerable => new StepProperty(propertyInfo.Name, arg2,
                            OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT2(enumerable.ToList())),
                        VariableName vn => new StepProperty(propertyInfo.Name, arg2, vn),
                        _ => Maybe<StepProperty>.None
                    };
                }
            }
        }



        private FreezableStepData FreezableStepData
        {
            get
            {
                var dict = AllProperties
                    .OrderBy(x => x.Index)
                    .ToDictionary(x => new StepParameterReference(x.Name),
                        x => x.Value.Match(
                            vn => new FreezableStepProperty(vn, new StepErrorLocation(this)),
                            s => new FreezableStepProperty(s.Unfreeze(),
                                new StepErrorLocation(this)),
                            sl =>
                                new FreezableStepProperty(
                                        sl.Select(s => s.Unfreeze()).ToImmutableList(), new StepErrorLocation(this))
                        ));

                return new FreezableStepData(dict, new StepErrorLocation(this));
            }
        }

        /// <inheritdoc />
        public IFreezableStep Unfreeze() => new CompoundFreezableStep(StepFactory.TypeName,FreezableStepData, Configuration);


        /// <summary>
        /// Check that this step meets requirements
        /// </summary>
        public virtual Result<Unit, IError> VerifyThis(ISettings settings) => Unit.Default;


        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings)
        {
            var r0 = new[] {VerifyThis(settings)};

            var rRequirements = RuntimeRequirements.Concat(StepFactory.Requirements)
                .Select(req => settings.CheckRequirement(req).MapError(x=>x.WithLocation(this)));

            var r3 = AllProperties
                .Select(x => x.Value.Match(
                    vn => Unit.Default,
                    s => s.Verify(settings),
                    sl => sl.Select(s => s.Verify(settings)).Combine(ErrorList.Combine).Map(_=>Unit.Default)

                ));



            var finalResult = r0.Concat(rRequirements).Concat(r3).Combine(ErrorList.Combine).Map(_ => Unit.Default);

            return finalResult;
        }
    }
}