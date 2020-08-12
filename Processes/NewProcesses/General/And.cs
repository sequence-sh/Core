using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{

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

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class AndProcessFactory : SimpleRunnableProcessFactory<And, bool>
    {
        private AndProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new AndProcessFactory();


        /// <inheritdoc />
        protected override string ProcessNameTemplate => $"[{nameof(And.Left)}] && [{nameof(And.Right)}]";



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
