using System;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Superpower;
using Superpower.Display;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Reductech.EDR.Utilities.Processes.mutable.injection
{
    internal interface IInjectionPath
    {
        IInjectionPath WithStep(IInjectionPath next);

        Result TrySetValue(object previous, string value);
    }

    internal enum PathToken
    {
        None = 0,
        [Token(Example = ".CasePath")]
        DotProperty,
        [Token(Example = "[Name]")]
        Indexer
    }

    static class InjectionParser
    {
        public static Result<IInjectionPath> TryParse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Result.Failure<IInjectionPath>("String is empty");

            if (!s.StartsWith(','))
                s = '.' + s;

            //try to split the search term up into tokens
            var tokensResult = Tokenizer.TryTokenize(s);

            if (!tokensResult.HasValue)
                return Result.Failure<IInjectionPath>(tokensResult.ToString());


            tokensResult.Value.ConsumeToken();

            var parseResult = FullProperty.TryParse(tokensResult.Value);

            if (!parseResult.HasValue)
                return Result.Failure<IInjectionPath>($"Could not parse '{s}'");

            if (!parseResult.Remainder.IsAtEnd)
                return Result.Failure<IInjectionPath>($"Could not parse all of '{s}'");

            return Result.Success(parseResult.Value);

        }

        private static readonly TokenListParser<PathToken, IInjectionPath> DotProperty =
            Token.EqualTo(PathToken.DotProperty)
                .Select(pt => new PropertyNameAccess(pt.ToStringValue()) as IInjectionPath);

        private static readonly TokenListParser<PathToken, IInjectionPath> Indexer =
            Token.EqualTo(PathToken.Indexer)
                .Select(pt => new IndexAccess(pt.ToStringValue()) as IInjectionPath);

        private static readonly TokenListParser<PathToken, IInjectionPath> FullProperty =
            DotProperty.Or(Indexer)
                .AtLeastOnce()
                .Select(ps => ps.Aggregate((a, b) => a.WithStep(b))).AtEnd();

        private static readonly Regex IndexerRegex = new Regex(@"\[(?<key>[_\-a-z0-9]+)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PropertyRegex = new Regex(@"\.(?<propertyName>[_\-a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static readonly Tokenizer<PathToken> Tokenizer
            = new TokenizerBuilder<PathToken>()
                .Match(Span.Regex(IndexerRegex.ToString(), IndexerRegex.Options), PathToken.Indexer)
                .Match(Span.Regex(PropertyRegex.ToString(), IndexerRegex.Options), PathToken.DotProperty)
                .Build();


        internal class PropertyNameAccess : IInjectionPath
        {
            public PropertyNameAccess(string propertyName)
            {
                PropertyName = propertyName.StartsWith('.') ? propertyName.Substring(1) : propertyName;
            }

            public string PropertyName { get; }

            public IInjectionPath? NextStep { get; private set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return $".{PropertyName}{NextStep}";
            }

            /// <inheritdoc />
            public IInjectionPath WithStep(IInjectionPath next)
            {
                if (NextStep == null)
                    NextStep = next;
                else
                    NextStep.WithStep(next);

                return this;
            }

            /// <inheritdoc />
            public Result TrySetValue(object previous, string value)
            {
                var property = previous.GetType().GetProperty(PropertyName);

                if (property == null)
                    return Result.Failure(
                        $"The type '{previous.GetType().Name}' does not have the property '{PropertyName}'");

                if (NextStep == null)
                {
                    object v;
                    if (property.PropertyType == typeof(string))
                    {
                        v = value;
                    }
                    else
                    {
                        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ??
                                             property.PropertyType;

                        string? error = null;
                        try
                        {
                            v = Convert.ChangeType(value, underlyingType);
                        }
                        catch (InvalidCastException e)
                        {
                            v = "";
                            error = e.Message;
                        }

                        if (error != null)
                        {
                            return Result.Failure($"Could not cast '{value}' to type '{underlyingType}'");
                        }
                    }

                    property.SetValue(previous, v);
                    return Result.Success();
                }
                else
                {
                    var thisPropertyValue = property.GetValue(previous);

                    if (thisPropertyValue == null)
                        return Result.Failure($"'{PropertyName}' is null");

                    return NextStep.TrySetValue(thisPropertyValue, value);
                }
            }
        }

        internal class IndexAccess : IInjectionPath
        {
            public IndexAccess(string indexTerm)
            {
                var match = IndexerRegex.Match(indexTerm);
                Index = match.Success ? match.Groups["key"].Value : indexTerm;
            }

            public string Index { get; }

            private static Result<object> TryConvertIndex(string index, Type parameterType)
            {
                if (parameterType == typeof(string))
                    return Result.Success<object>(index);

                if (parameterType == typeof(int))
                {
                    if (int.TryParse(index, out var n))
                        return Result.Success<object>(n);
                    return Result.Failure<object>($"Could not parse '{index}' to int");
                }
                return Result.Failure<object>($"Could not parse '{index}' to '{parameterType.Name}'");
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"[{Index}]{NextStep}";
            }

            public IInjectionPath? NextStep { get; private set; }

            /// <inheritdoc />
            public IInjectionPath WithStep(IInjectionPath next)
            {
                if (NextStep == null)
                    NextStep = next;
                else
                    NextStep.WithStep(next);


                return this;
            }

            /// <inheritdoc />
            public Result TrySetValue(object previous, string value)
            {
                var o = previous.GetType().GetProperty("Item");

                if (o == null)
                    return Result.Failure($"The type '{previous.GetType().Name}' cannot be accessed by index.");

                var parameterType = o.GetIndexParameters().FirstOrDefault()?.ParameterType;
                if (parameterType == null)
                    return Result.Failure($"The type '{previous.GetType().Name}' cannot be accessed by index.");

                var indexConvertResult = TryConvertIndex(Index, parameterType);

                if (indexConvertResult.IsFailure)
                    return indexConvertResult;

                if (NextStep == null)
                {
                    try
                    {
                        o.SetValue(previous, value, new[] {indexConvertResult.Value});
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
                    {
                        return Result.Failure(GetInnermostMessage(e));
                    }
#pragma warning restore CA1031 // Do not catch general exception types

                    return Result.Success();
                }

                object? nextObject;

                try
                {
                    nextObject = o.GetValue(previous, new[] {indexConvertResult.Value});
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    return Result.Failure(GetInnermostMessage(e));
                }
#pragma warning restore CA1031 // Do not catch general exception types

                return nextObject == null ?
                    Result.Failure($"Item at index '{Index}' is null.") :
                    NextStep.TrySetValue(nextObject, value);

                static string GetInnermostMessage(Exception e)
                {
                    return e.InnerException == null ? e.Message : GetInnermostMessage(e.InnerException);
                }
            }

        }

    }
}
