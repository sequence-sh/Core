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
using Reductech.EDR.Core.Internal.Logging;
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
    protected abstract Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    async Task<Result<T, IError>> IStep<T>.Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        using (stateMonad.Logger.BeginScope(Name))
        {
            IEnumerable<object> GetEnterStepArgs()
            {
                yield return Name;

                var properties = AllProperties
                    .ToDictionary(x => x.Name, x => x.GetLogName());

                yield return properties;
            }

            stateMonad.Logger.LogSituation(LogSituation.EnterStep, GetEnterStepArgs());

            var result = await Run(stateMonad, cancellationToken);

            if (result.IsFailure)
            {
                stateMonad.Logger.LogSituation(
                    LogSituation.ExitStepFailure,
                    Name,
                    result.Error.AsString
                );
            }
            else
            {
                var resultValue = SerializeOutput(result.Value);

                stateMonad.Logger.LogSituation(
                    LogSituation.ExitStepSuccess,
                    Name,
                    resultValue
                );
            }

            return result;

            static string SerializeOutput(object? o)
            {
                return o switch
                {
                    null            => "Null",
                    StringStream ss => ss.NameInLogs(false),
                    IArray array    => array.NameInLogs,
                    Unit            => "Unit",
                    _               => o.ToString()!
                };
            }
        }
    }

    /// <inheritdoc />
    public Task<Result<T1, IError>> Run<T1>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Run(stateMonad, cancellationToken)
            .BindCast<T, T1, IError>(
                ErrorCode.InvalidCast.ToErrorBuilder(typeof(T), typeof(T1)).WithLocation(this)
            );
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

    /// <summary>
    /// The text location for this step.
    /// </summary>
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public virtual IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            var allRequirements = AllProperties
                .SelectMany(x => x.RequiredVersions)
                .Select(x => x.ToRequirement());

            var groupedRequirements = Requirement.CompressRequirements(allRequirements);
            return groupedRequirements;
        }
    }

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
            var r = GetType()
                .GetProperties()
                .Select(
                    propertyInfo => (propertyInfo,
                                     attribute: propertyInfo
                                         .GetCustomAttribute<StepPropertyBaseAttribute>())
                )
                .Where(x => x.attribute != null)
                .OrderByDescending(x => x.attribute!.Order != null)
                .ThenBy(x => x.attribute!.Order)
                .SelectMany((x, i) => GetMember(x, i).ToEnumerable());

            return r;

            Maybe<StepProperty> GetMember(
                (PropertyInfo propertyInfo, StepPropertyBaseAttribute? attribute) arg1,
                int index)
            {
                var (propertyInfo, _) = arg1;
                var val = propertyInfo.GetValue(this);

                var oneOf =
                    val switch
                    {
                        IStep step =>
                            OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(step),
                        IEnumerable<IStep> enumerable =>
                            OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT2(
                                enumerable.ToList()
                            ),
                        VariableName vn => vn,
                        _               => null as OneOf<VariableName, IStep, IReadOnlyList<IStep>>?
                    };

                if (!oneOf.HasValue)
                    return Maybe<StepProperty>.None;

                var logAttribute = propertyInfo.GetCustomAttribute<LogAttribute>();

                var scopedFunctionAttribute =
                    propertyInfo.GetCustomAttribute<ScopedFunctionAttribute>();

                return new StepProperty(
                    propertyInfo.Name,
                    index,
                    oneOf.Value,
                    logAttribute,
                    scopedFunctionAttribute,
                    propertyInfo.GetCustomAttributes<RequiredVersionAttribute>().ToImmutableList()
                );
            }
        }
    }

    private FreezableStepData FreezableStepData
    {
        get
        {
            var dict = AllProperties
                .OrderBy(x => x.Index)
                .ToDictionary(
                    x => new StepParameterReference(x.Name),
                    x => x.Match(
                        vn => new FreezableStepProperty(vn,          TextLocation),
                        s => new FreezableStepProperty(s.Unfreeze(), TextLocation),
                        sl =>
                            new FreezableStepProperty(
                                sl.Select(s => s.Unfreeze()).ToImmutableList(),
                                TextLocation
                            )
                    )
                );

            return new FreezableStepData(dict, TextLocation);
        }
    }

    /// <inheritdoc />
    public IFreezableStep Unfreeze() => new CompoundFreezableStep(
        StepFactory.TypeName,
        FreezableStepData,
        Configuration,
        TextLocation
    );

    /// <summary>
    /// Check that this step meets requirements
    /// </summary>
    public virtual Result<Unit, IError> VerifyThis(SCLSettings settings) => Unit.Default;

    /// <inheritdoc />
    public virtual Result<TypeResolver, IError> TryGetScopedTypeResolver(
        TypeResolver baseTypeResolver,
        IFreezableStep scopedStep) => (SingleError)ErrorCode.CannotCreateScopedContext
        .ToErrorBuilder(Name)
        .WithLocation(this);

    /// <inheritdoc />
    public Result<Unit, IError> Verify(SCLSettings settings)
    {
        var r0 = new[] { VerifyThis(settings) };

        var rRequirements = StepFactory.Requirements.Concat(RuntimeRequirements)
            .Select(req => req.Check(settings).MapError(x => x.WithLocation(this)));

        var r3 = AllProperties
            .Select(
                x => x.Match(
                    _ => Unit.Default,
                    s => s.Verify(settings),
                    sl => sl.Select(s => s.Verify(settings))
                        .Combine(ErrorList.Combine)
                        .Map(_ => Unit.Default)
                )
            );

        var finalResult = r0.Concat(rRequirements)
            .Concat(r3)
            .Combine(ErrorList.Combine)
            .Map(_ => Unit.Default);

        return finalResult;
    }
}

}
