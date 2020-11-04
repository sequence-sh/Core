using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reductech.EDR.Core
{

    public enum EncodingEnum
    {
        Default,
        Ascii,
        BigEndianUnicode,
        UTF7,
        UTF8,
        UTF32,
        Unicode
    }

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
