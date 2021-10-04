using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

public static class DefaultValues
{
    public static T GetDefault<T>()
    {
        if (Unit.Default is T tUnit)
            return tUnit;

        if (StringStream.Empty is T tStringStream)
            return tStringStream;

        return default(T); //TODO do better
    }
}

}
