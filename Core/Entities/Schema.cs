using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// An entity schema.
/// Enforces that the entity matches certain constraints.
/// </summary>
[Serializable]
public sealed class Schema : IEntityConvertible
{
    /// <summary>
    /// The name of the schema.
    /// </summary>
    [JsonProperty]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The schema properties.
    /// </summary>
    [JsonProperty]
    public Dictionary<string, SchemaProperty> Properties { get; set; } = null!;

    /// <summary>
    /// Whether properties other than the explicitly defined properties are allowed.
    /// </summary>
    [JsonProperty]
    public ExtraPropertyBehavior ExtraProperties { get; set; } = ExtraPropertyBehavior.Allow;

    /// <summary>
    /// The default error behavior. This can be overriden by the individual properties or by the value passed to the EnforceSchema method.
    /// </summary>
    [JsonProperty]
    public ErrorBehavior DefaultErrorBehavior { get; set; } = ErrorBehavior.Fail;

    /// <inheritdoc />
    public override string ToString()
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        return Name;
    }

    /// <summary>
    /// Attempts to apply this schema to an entity.
    /// </summary>
    public Result<Maybe<Entity>, IErrorBuilder> ApplyToEntity(
        Entity entity,
        IStep callingStep,
        IStateMonad stateMonad,
        Maybe<ErrorBehavior> errorBehaviorOverride)
    {
        var remainingProperties = Properties
            .ToDictionary(
                x => x.Key,
                x => x.Value,
                StringComparer.OrdinalIgnoreCase
            );

        var newProperties = new List<EntityProperty>();
        var errors        = new List<IErrorBuilder>();
        var warnings      = new List<IErrorBuilder>();
        var changed       = false;
        var returnEntity  = true;

        void HandleError(IErrorBuilder error, ErrorBehavior eb)
        {
            switch (eb)
            {
                case ErrorBehavior.Fail:
                    errors.Add(error);
                    break;
                case ErrorBehavior.Error:
                    warnings.Add(error);
                    returnEntity = false;
                    break;
                case ErrorBehavior.Warning:
                    warnings.Add(error);
                    break;
                case ErrorBehavior.Skip:
                    returnEntity = false;
                    break;
                case ErrorBehavior.Ignore: break;
                default: throw new ArgumentOutOfRangeException(nameof(eb), eb, null);
            }
        }

        ErrorBehavior generalErrorBehavior;

        if (errorBehaviorOverride.HasValue)
            generalErrorBehavior = errorBehaviorOverride.Value;
        else
            generalErrorBehavior = this.DefaultErrorBehavior;

        foreach (var entityProperty in entity)
        {
            if (remainingProperties.Remove(entityProperty.Name, out var schemaProperty))
            {
                var convertResult = entityProperty.BestValue.TryConvert(schemaProperty);

                if (convertResult.IsSuccess)
                {
                    if (convertResult.Value.changed)
                    {
                        changed = true;

                        var newProperty = new EntityProperty(
                            entityProperty.Name,
                            entityProperty.BaseValue,
                            convertResult.Value.value,
                            entityProperty.Order
                        );

                        newProperties.Add(newProperty);
                    }
                    else
                    {
                        newProperties.Add(entityProperty);
                    }
                }

                else
                {
                    ErrorBehavior errorBehavior;

                    if (errorBehaviorOverride.HasValue)
                        errorBehavior = errorBehaviorOverride.Value;
                    else if (schemaProperty.ErrorBehavior != null)
                        errorBehavior = schemaProperty.ErrorBehavior.Value;
                    else
                        errorBehavior = DefaultErrorBehavior;

                    HandleError(convertResult.Error, errorBehavior);
                }
            }
            else
            {
                switch (ExtraProperties)
                {
                    case ExtraPropertyBehavior.Fail:
                    {
                        var errorBuilder = new ErrorBuilder(
                            ErrorCode.SchemaViolationUnexpectedProperty,
                            entityProperty.Name
                        );

                        HandleError(errorBuilder, ErrorBehavior.Fail);
                        break;
                    }
                    case ExtraPropertyBehavior.Remove:
                    {
                        //Do nothing
                        break;
                    }
                    case ExtraPropertyBehavior.Warn:
                    {
                        var errorBuilder = new ErrorBuilder(
                            ErrorCode.SchemaViolationUnexpectedProperty,
                            entityProperty.Name
                        );

                        HandleError(errorBuilder, ErrorBehavior.Warning);
                        break;
                    }
                    case ExtraPropertyBehavior.Allow:
                    {
                        newProperties.Add(entityProperty);
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        foreach (var (key, _) in remainingProperties
            .Where(
                x => x.Value.Multiplicity == Multiplicity.ExactlyOne
                  || x.Value.Multiplicity == Multiplicity.AtLeastOne
            ))
        {
            var error = new ErrorBuilder(ErrorCode.SchemaViolationMissingProperty, key);
            HandleError(error, generalErrorBehavior);
        }

        if (errors.Any())
        {
            var errorList = ErrorBuilderList.Combine(errors);
            return Result.Failure<Maybe<Entity>, IErrorBuilder>(errorList);
        }

        if (warnings.Any())
        {
            var warningList = ErrorBuilderList.Combine(warnings);

            LogSituation.SchemaViolation.Log(stateMonad, callingStep, warningList.AsString);
        }

        if (!returnEntity)
            return Maybe<Entity>.None;

        if (!changed)
            return Maybe<Entity>.From(entity);

        var resultEntity = new Entity(newProperties);

        return Maybe<Entity>.From(resultEntity);
    }
}

}
