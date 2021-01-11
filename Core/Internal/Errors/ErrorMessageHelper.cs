//using System;
//using System.Diagnostics;

//namespace Reductech.EDR.Core.Internal.Errors
//{

///// <summary>
///// Localizes core error messages
///// </summary>
//// ReSharper disable once InconsistentNaming
//public sealed class ErrorCodeHelper_Core_EN : IErrorCodeHelper<ErrorCode_Core>
//{
//    private ErrorCodeHelper_Core_EN() { }

//    /// <summary>
//    /// The instance
//    /// </summary>
//    public static IErrorCodeHelper<ErrorCode_Core> Instance { get; } =
//        new ErrorCodeHelper_Core_EN();

//    }

///// <summary>
///// Supports localization of error messages.
///// </summary>
//public interface IErrorCodeHelper<in TCode>
//    where TCode : Enum
//{
//    /// <summary>
//    /// Gets the format string for this error
//    /// </summary>
//    public string GetFormatString(TCode code);

//}

//}


