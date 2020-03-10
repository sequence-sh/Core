using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Reductech.EDR.Utilities.Processes
{

    /// <summary>
    /// Anything that implements ReadLineAsync
    /// </summary>
    internal interface IStreamReader<T> where T : struct
    {
        /// <summary>
        /// Reads a line of characters asynchronously and returns the data as a string and the source.
        /// </summary>
        /// <returns></returns>
        Task<T?> ReadLineAsync();
    }

    internal class StreamReaderWithSource<TEnum> : IStreamReader<(string line, TEnum source)>
        where TEnum : Enum
    {
        private readonly StreamReader _underlying;
        private readonly TEnum _source;

        public StreamReaderWithSource(StreamReader underlying, TEnum source)
        {
            _underlying = underlying;
            _source = source;
        }

        public async Task<(string line, TEnum source)?> ReadLineAsync()
        {
            var line = await _underlying.ReadLineAsync();

            if (line == null)
                return null;

            return (line, _source);
        }
    }

    /// <summary>
    /// Reads lines from several StreamReaders in the order that they arrive
    /// </summary>
    internal class MultiStreamReader<T> : IStreamReader<T>
    where T : struct
    {
        private readonly List<(IStreamReader<T> streamReader, Task<T?>? task)> _streamReaderTaskPairs;

        /// <summary>
        /// Create a new MultiStreamReader
        /// </summary>
        /// <param name="streamReaders"></param>
        public MultiStreamReader(IEnumerable<IStreamReader<T>> streamReaders)
        {
            _streamReaderTaskPairs = streamReaders.Select(x => (x, null as Task<T?>)).ToList();
        }

        /// <summary>
        /// Read the next line from any of these stream readers. Returns null if all of them are finished
        /// </summary>
        /// <returns></returns>
        public async Task<T?> ReadLineAsync()
        {
            var awaitingTasks = new List<Task<T?>>();

            for (var i = 0; i < _streamReaderTaskPairs.Count; i++) //go through all stream readers
            {
                var (streamReader, task1) = _streamReaderTaskPairs[i];
                if (task1 == null)
                {
                    //this stream reader has no yet been asked for the next line
                    var task = streamReader.ReadLineAsync();

                    _streamReaderTaskPairs[i] = (streamReader, task);

                    awaitingTasks.Add(task);
                }
                else
                {
                    //this stream reader has been asked for the next line during a previous call to this function
                    awaitingTasks.Add(task1);
                }
            }

            while (awaitingTasks.Any()) //loop until a string is returned that is not null
            {
                var firstCompletedTask = await Task.WhenAny(awaitingTasks);
                var firstResult = firstCompletedTask.Result;


                for (var i = 0; i < _streamReaderTaskPairs.Count; i++)
                {
                    var (streamReader, task) = _streamReaderTaskPairs[i];
                    if (task != firstCompletedTask) continue;
                    if (firstResult == null) _streamReaderTaskPairs.RemoveAt(i); //stream is finished - remove it
                    else _streamReaderTaskPairs[i] = (streamReader, null); //we've dealt with this task - remove it
                    break;
                }

                if (firstResult != null) return firstResult; //return this result

                awaitingTasks.Remove(firstCompletedTask); //remove this task
            }

            return null; //all readers are finished
        }
    }
}
