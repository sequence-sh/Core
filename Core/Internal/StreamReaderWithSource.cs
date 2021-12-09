using System;
using System.IO;
using System.Threading.Tasks;

namespace Reductech.EDR.Core.Internal;

internal class StreamReaderWithSource<TEnum> : IStreamReader<(string line, TEnum source)>
    where TEnum : Enum
{
    private readonly StreamReader _underlying;
    private readonly TEnum _source;

    public StreamReaderWithSource(StreamReader underlying, TEnum source)
    {
        _underlying = underlying;
        _source     = source;
    }

    public async Task<(string line, TEnum source)?> ReadLineAsync()
    {
        var line = await _underlying.ReadLineAsync();

        if (line == null)
            return null;

        return (line, _source);
    }
}
