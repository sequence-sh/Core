using System.Threading.Tasks;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Anything that implements ReadLineAsync
    /// </summary>
    public interface IStreamReader<T> where T : struct
    {
        /// <summary>
        /// Reads a line of characters asynchronously and returns the data as a string and the source.
        /// </summary>
        /// <returns></returns>
        Task<T?> ReadLineAsync();
    }
}