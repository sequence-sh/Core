namespace Reductech.EDR.Core.Internal.Errors
{
    public static class ErrorLocationHelper
    {
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStep step)=> errorBuilder.WithLocation(new StepErrorLocation(step));
        public static IError WithLocation(this IErrorBuilder errorBuilder, IFreezableStep step)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(step));
        public static IError WithLocation(this IErrorBuilder errorBuilder, IStepFactory factory, FreezableStepData data)=> errorBuilder.WithLocation(new FreezableStepErrorLocation(factory, data));

    }
}