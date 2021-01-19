//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using Result = CSharpFunctionalExtensions.Result;

//namespace Reductech.EDR.Core.Util.IDX
//{

//public static class Parser
//{
//    private static readonly Regex NamedTagRegex = new(
//        "#(?<TagName>DRE[A-Z]+)",
//        RegexOptions.Compiled | RegexOptions.IgnoreCase
//    );

//    private static readonly Regex FieldDataRegex = new(
//        @"(?<FieldName>\w+)\s*=\s*""(?<FieldValue>.+?)""",
//        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline
//    );

//    private static readonly Regex NamedTagValueRegex = new(
//        @".+?(?=#DRE)",
//        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline
//    );

//    public static CSharpFunctionalExtensions.Result<Entity> TryParse(string s)
//    {
//        try
//        {
//            //try to split the search term up into tokens
//            var tokensResult = Tokenizer.TryTokenize(s);

//            if (!tokensResult.HasValue)
//                return Result.Failure<Entity>(tokensResult.ToString());

//            var result = Record.TryParse(tokensResult.Value);

//            if (result.HasValue)
//                return Result.Success(result.Value);
//            else
//                return Result.Failure<Entity>(result.ToString());
//        }
//        #pragma warning disable CA1031 // Do not catch general exception types
//        catch (Exception e)
//        {
//            return Result.Failure<Entity>(e.Message);
//        }
//        #pragma warning restore CA1031 // Do not catch general exception types
//    }

//    private static readonly TokenListParser<IDXToken, Record> Record =
//        from referenceTag in Token.EqualTo(IDXToken.ReferenceTag)
//        from referenceId in Token.EqualTo(IDXToken.NamedTagValue)
//        from otherFields in (RegularField.Or(NamedTagField)).Many()
//        from endDoc in Token.EqualTo(IDXToken.EndDoc)
//        from endDataReference in Token.EqualTo(IDXToken.EndDataReference)
//        select CreateRecord(referenceId, otherFields);

//    private static readonly Tokenizer<IDXToken> Tokenizer =
//        new TokenizerBuilder<IDXToken>()
//            .Ignore(Span.WhiteSpace)
//            .Match(Span.EqualToIgnoreCase("#DREREFERENCE"), IDXToken.ReferenceTag)
//            .Match(Span.EqualToIgnoreCase("#DREFIELD"), IDXToken.FieldTag)
//            .Match(Span.EqualToIgnoreCase("#DREENDDOC"), IDXToken.EndDoc)
//            .Match(Span.EqualToIgnoreCase("#DREENDDATAREFERENCE"), IDXToken.EndDataReference)
//            .Match(Span.Regex(NamedTagRegex.ToString(), NamedTagRegex.Options), IDXToken.NamedTag)
//            .Match(
//                Span.Regex(FieldDataRegex.ToString(), FieldDataRegex.Options),
//                IDXToken.FieldData
//            )
//            //.Match(Span.WithAll(x=> char.IsNumber(x) || char.IsLetter(x)), IDXToken.ReferenceId)
//            .Match(
//                Span.Regex(NamedTagValueRegex.ToString(), NamedTagValueRegex.Options),
//                IDXToken.NamedTagValue
//            )
//            .Build();

//    private static readonly TokenListParser<IDXToken, KeyValuePair<string, string>> RegularField =
//        from f in Token.EqualTo(IDXToken.FieldTag)
//        from v in Token.EqualTo(IDXToken.FieldData)
//        select CreateKeyValuePairFromFieldData(v);

//    private static readonly TokenListParser<IDXToken, KeyValuePair<string, string>> NamedTagField =
//        from f in Token.EqualTo(IDXToken.NamedTag)
//        from v in Token.EqualTo(IDXToken.NamedTagValue)
//        select CreateKeyValuePairFromNamedTag(f, v);

//    private static KeyValuePair<string, string> CreateKeyValuePairFromFieldData(
//        Token<IDXToken> token)
//    {
//        var match = FieldDataRegex.Match(token.ToStringValue());

//        if (match.Success)
//        {
//            return new KeyValuePair<string, string>(
//                match.Groups["FieldName"].Value.Trim(),
//                match.Groups["FieldValue"].Value.Trim()
//            );
//        }

//        throw new ArgumentException("Could not match field data token");
//    }

//    private static KeyValuePair<string, string> CreateKeyValuePairFromNamedTag(
//        Token<IDXToken> namedTag,
//        Token<IDXToken> namedTagValue)
//    {
//        var namedTagMatch = NamedTagRegex.Match(namedTag.ToStringValue().Trim());

//        if (namedTagMatch.Success)
//        {
//            return new KeyValuePair<string, string>(
//                namedTagMatch.Groups["TagName"].Value,
//                namedTagValue.ToStringValue().Trim()
//            );
//        }

//        throw new ArgumentException("Could not match named tag");
//    }

//    private static Entity CreateRecord(
//        Token<IDXToken> reference,
//        IEnumerable<KeyValuePair<string, string>> otherFieldData)
//    {
//        var keyValuePairs = new[]
//        {
//            new KeyValuePair<string, string>("DREREFERENCE", reference.ToStringValue().Trim())
//        }.Concat(otherFieldData);

//        return new Entity(keyValuePairs);
//    }
//}

//}


