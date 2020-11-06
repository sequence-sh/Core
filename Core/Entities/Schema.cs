using System;
using System.Collections.Generic;
using System.Text;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// An entity schema.
    /// Enforces that the entity matches certain constraints.
    /// </summary>
    public sealed class Schema
    {
    }

    public enum Multiplicity
    {
        Any,
        AtLeastOne,
        ExactlyOne,
        UpToOne
    }

    public enum PropertySchemaType
    {
        String,
        Integer,
        Double,
        Enum,
        Bool,
        Date

    }
}
