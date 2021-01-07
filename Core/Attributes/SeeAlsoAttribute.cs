using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Indicates a related topic.
/// </summary>
public sealed class SeeAlsoAttribute : Attribute
{
    /// <summary>
    /// Creates a new SeeAlsoAttribute
    /// </summary>
    /// <param name="seeAlso"></param>
    public SeeAlsoAttribute(string seeAlso)
    {
        SeeAlso = seeAlso;
    }

    /// <summary>
    /// EntityStreamFilter to go to see something else.
    /// </summary>
    public string SeeAlso { get; }
}

}
