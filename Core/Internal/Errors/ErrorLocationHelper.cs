using System.Collections.Immutable;
using System.Linq;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// Extension methods for ErrorBuilders that involve adding a location.
    /// </summary>
    public static class ErrorLocationHelper
    {
        /// <summary>
        /// Combines two error locations
        /// </summary>
        public static IErrorLocation Combine(this IErrorLocation l1, IErrorLocation l2)
        {
            if (l1 is EmptyLocation) return l2;
            if(l2 is EmptyLocation) return l1;


            if (l1 is ErrorLocationList ell1)
            {
                if(l2 is ErrorLocationList ell2)
                    return new ErrorLocationList(ell1.Locations.AddRange(ell2.Locations));

                return new ErrorLocationList(ell1.Locations.Add(l2));
            }

            else if (l2 is ErrorLocationList ell2)
            {
                return new ErrorLocationList(ell2.Locations.Prepend(l1).ToImmutableList());
            }
            else return new ErrorLocationList(ImmutableList<IErrorLocation>.Empty.Add(l1).Add(l2));

        }


        /// <summary>
        /// Add a StepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStep step)=> errorBuilder.WithLocation(new StepErrorLocation(step));

        /// <summary>
        /// Add a FreezableStepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IFreezableStep step)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(step));

        /// <summary>
        /// Add a FreezableStepErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStepFactory factory, FreezableStepData data)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(factory, data));

        /// <summary>
        /// Add a YamlRegionErrorLocation
        /// </summary>
        public static IError WithLocation(this IErrorBuilder errorBuilder, TextPosition token) => errorBuilder.WithLocation(token);
    }
}