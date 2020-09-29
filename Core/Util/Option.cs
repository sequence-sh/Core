using System;
using System.Data;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Util
{
    /// <summary>
    /// One of two possible objects.
    /// </summary>
    public sealed class Option<T1, T2>
    {
        /// <summary>
        /// Creates a new option
        /// </summary>
        public Option(T1 t1)
        {
            Choice1 = Maybe<T1>.From(t1);
            Choice2 = Maybe<T2>.None;
        }

        /// <summary>
        /// Creates a new option
        /// </summary>
        public Option(T2 t2)
        {
            Choice1 = Maybe<T1>.None;
            Choice2 = Maybe<T2>.From(t2);
        }

        /// <summary>
        /// The first choice if it is set.
        /// </summary>
        public Maybe<T1> Choice1 { get; }

        /// <summary>
        /// The second choice if it is set.
        /// </summary>
        public Maybe<T2> Choice2 { get; }

        /// <summary>
        /// Use this option
        /// </summary>
        public TResult Join<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2)
        {
            if (Choice1.HasValue)
                return f1(Choice1.Value);
            if (Choice2.HasValue)
                return f2(Choice2.Value);

            throw new StrongTypingException("No options are set.");
        }

        /// <inheritdoc />
        public override string? ToString() => Join(x => x?.ToString(), x => x?.ToString());


        /// <summary>
        /// Determines if this is equal to another object.
        /// </summary>
        public bool Equals(Option<T1, T2> other, Func<T1, T1, bool> equals1, Func<T2, T2, bool> equals2)
        {
            if (Choice1.HasValue && other.Choice1.HasValue)
                return equals1(Choice1.Value, other.Choice1.Value);

            if (Choice2.HasValue && other.Choice2.HasValue)
                return equals2(Choice2.Value, other.Choice2.Value);

            return false;
        }
    }



    /// <summary>
    /// One of three possible objects.
    /// </summary>
    public sealed class Option<T1, T2, T3>
    {
        /// <summary>
        /// Creates a new option
        /// </summary>
        public Option(T1 t1)
        {
            Choice1 = Maybe<T1>.From(t1);
            Choice2 = Maybe<T2>.None;
            Choice3 = Maybe<T3>.None;
        }

        /// <summary>
        /// Creates a new option
        /// </summary>
        public Option(T2 t2)
        {
            Choice1 = Maybe<T1>.None;
            Choice2 = Maybe<T2>.From(t2);
            Choice3 = Maybe<T3>.None;
        }

        /// <summary>
        /// Creates a new option
        /// </summary>
        public Option(T3 t3)
        {
            Choice1 = Maybe<T1>.None;
            Choice2 = Maybe<T2>.None;
            Choice3 = Maybe<T3>.From(t3);
        }

        /// <summary>
        /// The first choice if it is set.
        /// </summary>
        public Maybe<T1> Choice1 { get; }

        /// <summary>
        /// The second choice if it is set.
        /// </summary>
        public Maybe<T2> Choice2 { get; }

        /// <summary>
        /// The third choice if it is set.
        /// </summary>
        public Maybe<T3> Choice3 { get; }

        /// <summary>
        /// Use this option
        /// </summary>
        public TResult Join<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2, Func<T3, TResult> f3)
        {
            if (Choice1.HasValue)
                return f1(Choice1.Value);
            if (Choice2.HasValue)
                return f2(Choice2.Value);
            if (Choice3.HasValue)
                return f3(Choice3.Value);


            throw new StrongTypingException("No options are set.");
        }

        /// <inheritdoc />
        public override string? ToString() => Join(x => x?.ToString(), x => x?.ToString(), x=>x?.ToString());

        /// <summary>
        /// Determines if this is equal to another object.
        /// </summary>
        public bool Equals(Option<T1, T2, T3> other, Func<T1, T1, bool> equals1, Func<T2, T2, bool> equals2, Func<T3, T3, bool> equals3)
        {
            if (Choice1.HasValue && other.Choice1.HasValue)
                return equals1(Choice1.Value, other.Choice1.Value);

            if (Choice2.HasValue && other.Choice2.HasValue)
                return equals2(Choice2.Value, other.Choice2.Value);

            if (Choice3.HasValue && other.Choice3.HasValue)
                return equals3(Choice3.Value, other.Choice3.Value);

            return false;
        }
    }
}
