using System.Globalization;

namespace Sequence.Core.Entities.Schema;

/// <summary>
/// Options for converting string values to schema nodes
/// </summary>
public record SchemaConversionOptions(
    Formatter? DateInputFormats,
    Formatter? BoolTrueFormats,
    Formatter? BoolFalseFormats,
    Formatter? NullFormats,
    bool CaseSensitive)
{
    /// <summary>
    /// Gets a node which matches the value.
    /// </summary>
    public SchemaNode GetNode(string value, string path)
    {
        if (DateInputFormats is not null)
        {
            foreach (var format in DateInputFormats.GetFormats(path.TrimStart('/')))
            {
                if (DateTime.TryParseExact(
                        value,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _
                    ))
                {
                    return new StringNode(
                        EnumeratedValuesNodeData.Empty,
                        DateTimeStringFormat.Instance,
                        StringRestrictions.NoRestrictions
                    );
                }
            }
        }

        if (BoolTrueFormats?.IsMatch(value, path, CaseSensitive) == true
         || BoolFalseFormats?.IsMatch(value, path, CaseSensitive) == true)
            return new BooleanNode(EnumeratedValuesNodeData.Empty);

        if (NullFormats?.IsMatch(value, path, CaseSensitive) == true)
            return new NullNode();

        if (int.TryParse(value, out _))
            return IntegerNode.Default;

        if (double.TryParse(value, out _))
            return NumberNode.Default;

        return StringNode.Default;
    }
}
