using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that creates and returns an entity.
    /// </summary>
    public class CreateEntityStep : IStep<Entity>
    {
        /// <summary>
        /// Create a new CreateEntityStep
        /// </summary>
        /// <param name="properties"></param>
        public CreateEntityStep(IReadOnlyDictionary<string, IStep> properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// The entity properties
        /// </summary>
        public IReadOnlyDictionary<string, IStep> Properties { get; }

        /// <inheritdoc />
        public async Task<Result<Entity, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var pairs = new List<KeyValuePair<string, object>>();

            foreach (var (key, step) in Properties)
            {
                var r = await step.Run<object>(stateMonad, cancellationToken);

                if (r.IsFailure) return r.ConvertFailure<Entity>();

                pairs.Add(new KeyValuePair<string, object>(key, r.Value));
            }

            return Entity.Create(pairs);
        }

        /// <inheritdoc />
        public string Name => "Create Entity";

        /// <inheritdoc />
        public IFreezableStep Unfreeze()
        {
            var dictionary =
            Properties.ToDictionary(x => x.Key, x =>
                new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(x.Value.Unfreeze()),
                    new StepErrorLocation(x.Value) ));

            return new CreateEntityFreezableStep(new FreezableStepData(dictionary, new StepErrorLocation(this)));
        }

        /// <inheritdoc />
        public async Task<Result<T, IError>> Run<T>(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Run(stateMonad, cancellationToken).BindCast<Entity, T, IError>(
                    new SingleError($"Could not cast {typeof(Entity)} to {typeof(T)}", ErrorCode.InvalidCast, new StepErrorLocation(this)));
        }

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings)
        {
            var r = Properties.Select(x => x.Value.Verify(settings))
                .Combine(_ => Unit.Default, ErrorList.Combine);

            return r;
        }

        /// <inheritdoc />
        public Configuration? Configuration { get; set; } = null;

        /// <inheritdoc />
        public Type OutputType => typeof(Entity);

        /// <inheritdoc />
        public async Task<string> SerializeAsync(CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();

            sb.Append('(');

            var results = new List<string>();

            foreach (var (key, value) in Properties)
            {
                var valueString = await value.SerializeAsync(cancellationToken);

                results.Add($"{key}: {valueString}");
            }

            sb.AppendJoin(",", results);

            sb.Append(')');

            return sb.ToString();
        }
    }
}