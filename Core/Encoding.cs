﻿using System;
using System.Text;

namespace Reductech.EDR.Core
{

    /// <summary>
    /// An encoding
    /// </summary>
    public enum EncodingEnum
    {
        /// <summary>
        /// The default encoding.
        /// </summary>
        Default,
        /// <summary>
        /// Ascii
        /// </summary>
        Ascii,
        /// <summary>
        /// Unicode with big-endian byte order
        /// </summary>
        BigEndianUnicode,
        /// <summary>
        /// UTF7
        /// </summary>
        UTF7,
        /// <summary>
        /// UTF8
        /// </summary>
        UTF8,
        /// <summary>
        /// UTF32
        /// </summary>
        UTF32,
        /// <summary>
        /// Unicode with little-endian byte order
        /// </summary>
        Unicode
    }

    /// <summary>
    /// Contains methods for converting encodings.
    /// </summary>
    public static class EncodingHelper
    {
        /// <summary>
        /// Convert this to a System.Text.Encoding.
        /// </summary>
        public static Encoding Convert(this EncodingEnum encodingEnum)
        {
            return encodingEnum switch
            {
                EncodingEnum.Default => Encoding.Default,
                EncodingEnum.Ascii => Encoding.ASCII,
                EncodingEnum.BigEndianUnicode => Encoding.BigEndianUnicode,
                EncodingEnum.UTF7 => Encoding.UTF7,
                EncodingEnum.UTF8 => Encoding.UTF8,
                EncodingEnum.UTF32 => Encoding.UTF32,
                EncodingEnum.Unicode => Encoding.Unicode,
                _ => throw new ArgumentOutOfRangeException(nameof(encodingEnum), encodingEnum, null)
            };
        }
    }
}