using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// Immutable version of a conditional process.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Conditional<T> : ImmutableProcess<T>
    {
        /// <inheritdoc />
        public Conditional(IImmutableProcess<bool> @if, IImmutableProcess<T> then, IImmutableProcess<T> @else)
        {
            If = @if;
            Then = then;
            Else = @else;
        }
        /// <summary>
        /// The process determining which branch should be chosen.
        /// </summary>
        public readonly IImmutableProcess<bool> If;

        /// <summary>
        /// The process to run if the condition is successful.
        /// </summary>
        public readonly IImmutableProcess<T> Then;
        /// <summary>
        /// The process to run if the condition is unsuccessful.
        /// </summary>
        public readonly IImmutableProcess<T> Else;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<T>> Execute()
        {
            //yield return ProcessOutput<T>.Message($"Testing {If}");

            bool? success = null;
            var anyErrors = false;

            await foreach (var r in If.Execute())
            {
                if (r.OutputType == OutputType.Success)
                    success = r.Value;
                else
                {
                    if (r.OutputType == OutputType.Error)
                        anyErrors = true;

                    yield return r.ConvertTo<T>(); //These methods failing is expected so it should not produce an error
                }
            }

            if (!anyErrors)
            {
                if (success.HasValue)
                {
                    if (success.Value)
                    {
                        //yield return ProcessOutput<T>.Message($"Condition met, executing {Then}");

                        await foreach (var r in Then.Execute())
                            yield return r;
                    }
                    else if (Else != null)
                    {
                        //yield return ProcessOutput<T>.Message($"Condition not met, executing {Else}");

                        await foreach (var r in Else.Execute())
                            yield return r;
                    }
                }
                else
                    yield return ProcessOutput<T>.Error("Could not determine result of conditional");
            }


        }

        /// <inheritdoc />
        public override string Name =>
            ProcessNameHelper.GetConditionalName(If.Name, Then.Name, Else?.Name);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}