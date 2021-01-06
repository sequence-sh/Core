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
using Reductech.EDR.Core.Logging;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A runnable step that is not a constant.
    /// </summary>
    public abstract class CompoundStep<T> : ICompoundStep<T>
    {
        /// <summary>
        /// Run this step.
        /// Does not activate logging.
        /// </summary>
        protected abstract Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken);


        /// <inheritdoc />
        async Task<Result<T, IError>> IStep<T>.Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            using (stateMonad.Logger.BeginScope(Name))
            {
                IEnumerable<object> GetEnterStepArgs()
                {
                    yield return Name;

                    var properties = AllProperties
                        .ToDictionary(x => x.Name, x=>x.GetLogName());

                    yield return properties;
                }

                stateMonad.Logger.LogSituation(LogSituationCore.EnterStep, GetEnterStepArgs());

                var result = await Run(stateMonad, cancellationToken);

                if (result.IsFailure)
                {
                    stateMonad.Logger.LogSituation(LogSituationCore.ExitStepFailure, new[]{Name, result.Error.AsString});
                }
                else
                {
                    var resultValue = SerializeOutput(result.Value);
                    stateMonad.Logger.LogSituation(LogSituationCore.ExitStepSuccess, new[]{Name, resultValue});
                }
                return result;

                static string SerializeOutput(object? o)
                {
                    return o switch
                    {
                        null => "Null",
                        StringStream ss => ss.NameInLogs(false),
                        IArray array => array.NameInLogs,
                        Unit => "Unit",
                        _ => o.ToString()!
                    };
                }

                
            }
        }

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
        public virtual string Name => StepFactory.TypeName;

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

                    OneOf<VariableName, IStep, IReadOnlyList<IStep>>? oneOf =
                        val switch
                    {
                        IStep step =>
                            OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(step),
                        IEnumerable<IStep> enumerable =>
                            OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT2(enumerable.ToList()),
                        VariableName vn =>  vn,
                        _ => default
                    };

                    if(!oneOf.HasValue)
                        return Maybe<StepProperty>.None;

                    var logAttribute = propertyInfo.GetCustomAttribute<LogAttribute>();
                    var scopedFunctionAttribute = propertyInfo.GetCustomAttribute<ScopedFunctionAttribute>();

                    return new StepProperty(propertyInfo.Name, arg2, oneOf.Value, logAttribute, scopedFunctionAttribute);
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
                        x => x.Match(
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
        public virtual Result<StepContext, IError> TryGetScopedContext(StepContext baseContext, IFreezableStep scopedStep) =>
            new SingleError($"{Name} cannot create a scoped context", ErrorCode.UnexpectedParameter, new StepErrorLocation(this));

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings)
        {
            var r0 = new[] {VerifyThis(settings)};

            var rRequirements = RuntimeRequirements.Concat(StepFactory.Requirements)
                .Select(req => settings.CheckRequirement(req).MapError(x=>x.WithLocation(this)));

            var r3 = AllProperties
                .Select(x => x.Match(
                    _ => Unit.Default,
                    s => s.Verify(settings),
                    sl => sl.Select(s => s.Verify(settings)).Combine(ErrorList.Combine).Map(_=>Unit.Default)

                ));



            var finalResult = r0.Concat(rRequirements).Concat(r3).Combine(ErrorList.Combine).Map(_ => Unit.Default);

            return finalResult;
        }
    }
}