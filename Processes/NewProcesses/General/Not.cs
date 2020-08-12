using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public class NotProcessFactory : SimpleRunnableProcessFactory<NotProcessFactory.Not, bool>
    {
        private NotProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new NotProcessFactory();


        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"Not [{nameof(Not.Boolean)}]";


        /// <summary>
        /// Negation of a boolean value.
        /// </summary>
        public sealed class Not : CompoundRunnableProcess<bool>
        {
            /// <inheritdoc />
            public override Result<bool> Run(ProcessState processState) => Boolean.Run(processState).Map(x => !x);

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => NotProcessFactory.Instance;

            /// <summary>
            /// The value to negate.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Boolean { get; set; }
        }
    }

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class AndProcessFactory : SimpleRunnableProcessFactory<AndProcessFactory.And, bool>
    {
        private AndProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new AndProcessFactory();


        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"[{nameof(And.Left)}] && [{nameof(And.Right)}]";


        /// <summary>
        /// Returns true if both operands are true
        /// </summary>
        public sealed class And : CompoundRunnableProcess<bool>
        {
            /// <summary>
            /// The left operand. Will always be evaluated.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Left { get; set; }


            /// <summary>
            /// The right operand. Will not be evaluated unless the left operand is true.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Right { get; set; }

            /// <inheritdoc />
            public override Result<bool> Run(ProcessState processState) => Left.Run(processState).Bind(x => x ? Right.Run(processState) : false);

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => AndProcessFactory.Instance;
        }
    }


    /// <summary>
    /// Returns true if either operand is true
    /// </summary>
    public sealed class OrProcessFactory : SimpleRunnableProcessFactory<OrProcessFactory.Or, bool>
    {
        private OrProcessFactory() { }

        public static SimpleRunnableProcessFactory<Or, bool> Instance { get; } = new OrProcessFactory();


        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"[{nameof(Or.Left)}] || [{nameof(Or.Right)}]";

        /// <summary>
        /// Returns true if either operand is true
        /// </summary>
        public sealed class Or : CompoundRunnableProcess<bool>
        {
            /// <summary>
            /// The left operand. Will always be evaluated.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Left { get; set; }


            /// <summary>
            /// The right operand. Will not be evaluated unless the left operand is false.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Right { get; set; }

            /// <inheritdoc />
            public override Result<bool> Run(ProcessState processState) => Left.Run(processState).Bind(x => x ? true : Right.Run(processState));

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => OrProcessFactory.Instance;
        }


    }


    /// <summary>
    /// Repeat a process a set number of times.
    /// </summary>
    public sealed class RepeatXTimesProcessFactory : SimpleRunnableProcessFactory<RepeatXTimesProcessFactory.RepeatXTimes, Unit>
    {
        private RepeatXTimesProcessFactory() { }

        public static SimpleRunnableProcessFactory<RepeatXTimes, Unit> Instance { get; } = new RepeatXTimesProcessFactory();

        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"Repeat '[{nameof(RepeatXTimes.Action)}]' '[{nameof(RepeatXTimes.Number)}]' times.";

        /// <summary>
        /// Repeat a process a set number of times.
        /// </summary>
        public sealed class RepeatXTimes : CompoundRunnableProcess<Unit>
        {
            /// <summary>
            /// The action to perform repeatedly.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<Unit> Action { get; set; }

            /// <summary>
            /// The number of times to perform the action.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<int> Number { get; set; }

            /// <inheritdoc />
            public override Result<Unit> Run(ProcessState processState)
            {
                var numberResult = Number.Run(processState);

                if (numberResult.IsFailure) return numberResult.ConvertFailure<Unit>();

                for (var i = 0; i < numberResult.Value; i++)
                {
                    var result = Action.Run(processState);
                    if (result.IsFailure) return result.ConvertFailure<Unit>();
                }

                return Unit.Default;
            }

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => Instance;
        }
    }


    /// <summary>
    /// Repeat an action while the condition is met.
    /// </summary>
    public sealed class RepeatWhileProcessFactory : SimpleRunnableProcessFactory<RepeatWhileProcessFactory.RepeatWhile, Unit>
    {
        private RepeatWhileProcessFactory() { }

        public static SimpleRunnableProcessFactory<RepeatWhile, Unit> Instance { get; } = new RepeatWhileProcessFactory();

        /// <summary>
        /// Repeat an action while the condition is met.
        /// </summary>
        public sealed class RepeatWhile : CompoundRunnableProcess<Unit>
        {
            /// <inheritdoc />
            public override Result<Unit> Run(ProcessState processState)
            {
                while (true)
                {
                    var conditionResult = Condition.Run(processState);
                    if (conditionResult.IsFailure) return conditionResult.ConvertFailure<Unit>();

                    if (conditionResult.Value)
                    {
                        var actionResult = Action.Run(processState);
                        if (actionResult.IsFailure) return actionResult.ConvertFailure<Unit>();
                    }
                    else break;
                }

                return Unit.Default;
            }

            /// <summary>
            /// The action to perform repeatedly.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<Unit> Action { get; set; }


            /// <summary>
            /// The condition to check before performing the action.
            /// </summary>
            [RunnableProcessProperty]
            [Required]
            public IRunnableProcess<bool> Condition { get; set; }

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => Instance;
        }

        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"Repeat '[{nameof(RepeatWhile.Action)}]' while '[{nameof(RepeatWhile.Condition)}]'";
    }


    //public sealed class CountWith : CompoundRunnableProcess<Unit>
    //{

    //}

    //public sealed class ForEach : CompoundRunnableProcess<Unit>
    //{

    //}

    //public sealed class BreakFromLoop : CompoundRunnableProcess<Unit>
    //{

    //}

    //public sealed class ContinueWithLoop : CompoundRunnableProcess<Unit>
    //{

    //}

    //public sealed class ApplyOperator : CompoundRunnableProcess<int>
    //{

    //}

    //public enum MathOperator
    //{
    //    Plus,
    //    Minus,
    //    Times,
    //    Divide,
    //    ToThePowerOf

    //}

    //public sealed class JoinStrings : CompoundRunnableProcess<string>
    //{

    //}

    ///// <summary>
    ///// Appends a string to an existing string variable
    ///// </summary>
    //public sealed class AppendString : CompoundRunnableProcess<string>
    //{

    //}

    //public sealed class LengthOfString : CompoundRunnableProcess<int>
    //{

    //}

    //public sealed class StringIsEmpty : CompoundRunnableProcess<bool>
    //{

    //}

    //public sealed class FirstIndexOf : CompoundRunnableProcess<int>
    //{

    //}

    //public sealed class LastIndexOf : CompoundRunnableProcess<int>
    //{

    //}

    //public sealed class GetLetterAtIndex : CompoundRunnableProcess<string>
    //{

    //}

    //public sealed class GetSubstring : CompoundRunnableProcess<string>
    //{

    //}

    //public sealed class ToCase : CompoundRunnableProcess<string>
    //{

    //}

    //public enum TextCase
    //{
    //    Upper,
    //    Lower,
    //    Title
    //}

    //public sealed class Trim : CompoundRunnableProcess<string>
    //{


    //}

    //public enum TrimSide
    //{
    //    Left,
    //    Right,
    //    Both
    //}

    ////TODO prompt

    //public sealed class Repeat<T> : CompoundRunnableProcess<List<T>>
    //{

    //}

    //public sealed class ArrayCount : CompoundRunnableProcess<int>
    //{

    //}

    //public sealed class ArrayIsEmpty : CompoundRunnableProcess<bool>
    //{

    //}

    //public sealed class FirstIndexOf : CompoundRunnableProcess<int>
    //{

    //}

    //public sealed class ElementAtIndex<T> : CompoundRunnableProcess<T>
    //{

    //}

    //public sealed class SetElement<T> : CompoundRunnableProcess<Unit>
    //{

    //}

    //public sealed class SubList<T> : CompoundRunnableProcess<List<T>>
    //{
    //    //From and to indexes
    //}

    //public sealed class MakeListFromText: CompoundRunnableProcess<List<string>>
    //{
    //    //delimiter
    //}

    //public sealed class SortList<T> : CompoundRunnableProcess<List<T>>
    //{
    //    //From and to indexes
    //}

    //public enum SortOrder
    //{
    //    Ascending,
    //    Descending
    //}

    //public sealed class IncrementVariable : CompoundRunnableProcess<Unit>
    //{
    //    //Include amount to increment by
    //}

}
