using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Processes;

namespace ProcessesTests
{
    public class MultiStreamReaderTest
    {

        public enum StreamReaderSource
        {
            StreamReaderOne,
            StreamReaderTwo,
            StreamReaderThree
        }

        [Test]
        public async Task TestReadingEmptyStream()
        {
            var reader1 = new Mock<IStreamReader<(string, StreamReaderSource)>>(MockBehavior.Strict);
            var multiStreamReader = new MultiStreamReader<(string, StreamReaderSource)>(new[] { reader1.Object });

            reader1.Setup(m => m.ReadLineAsync()).ReturnsAsync(null as (string, StreamReaderSource)?);

            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);

            reader1.VerifyAll();
        }

        [Test]
        public async Task TestReadingTextFromStream()
        {
            var reader1 = new Mock<IStreamReader<(string, StreamReaderSource)>>(MockBehavior.Strict);
            var multiStreamReader = new MultiStreamReader<(string, StreamReaderSource)>(new[] { reader1.Object });

            foreach (var str in new[]{String1, String2, String3})
            {
                var pair = (str, SROne: StreamReaderSource.StreamReaderOne);

                reader1.Setup(m => m.ReadLineAsync()).ReturnsAsync(pair);
                var a = await multiStreamReader.ReadLineAsync();
                Assert.AreEqual(pair, a);
                reader1.VerifyAll();
            }

            reader1.Setup(m => m.ReadLineAsync()).ReturnsAsync(null as (string, StreamReaderSource)?);
            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);

            reader1.VerifyAll();
        }

        private const string String1 = "string 1";
        private const string String2 = "string 2";
        private const string String3 = "string 3";

        [Test]
        public async Task TestReadingTwoStreams()
        {
            var reader1 = new Mock<IStreamReader<(string, StreamReaderSource)>>(MockBehavior.Strict);
            var reader2 = new Mock<IStreamReader<(string, StreamReaderSource)>>(MockBehavior.Strict);

            var semaphore1 = new SemaphoreSlim(1);
            semaphore1.Wait();
            var semaphore2 = new SemaphoreSlim(1);
            semaphore2.Wait();

            reader1.Setup(m => m.ReadLineAsync())
                .Returns(()=> ReturnsEntityAfter<(string, StreamReaderSource)?>((String1, StreamReaderSource.StreamReaderOne), semaphore1));

            reader2.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnsEntityAfter<(string, StreamReaderSource)?>((String3, StreamReaderSource.StreamReaderThree), semaphore2));
            

            var multiStreamReader = new MultiStreamReader<(string, StreamReaderSource)>(new[] {reader1.Object, reader2.Object});

            semaphore1.Release();

            var r1 = await multiStreamReader.ReadLineAsync();
            Assert.AreEqual((String1, StreamReaderSource.StreamReaderOne), r1);

            reader1.VerifyAll();
            reader2.VerifyAll();

            reader1.Setup(m => m.ReadLineAsync())
                .ReturnsAsync((String2,StreamReaderSource.StreamReaderTwo));

            var r2 = await multiStreamReader.ReadLineAsync();
            Assert.AreEqual((String2, StreamReaderSource.StreamReaderTwo), r2);
            reader1.VerifyAll();
            reader2.VerifyAll();

            reader1.Setup(m => m.ReadLineAsync())
                .ReturnsAsync(null as (string, StreamReaderSource)?);

            semaphore2.Release();
                
            var r3 = await multiStreamReader.ReadLineAsync();

            Assert.AreEqual((String3, StreamReaderSource.StreamReaderThree), r3);
            reader1.VerifyAll();
            reader2.VerifyAll();

            reader2.Setup(m => m.ReadLineAsync())
                .ReturnsAsync(null as (string, StreamReaderSource)?);


            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);
        }


        private static async Task<T> ReturnsEntityAfter<T>(T entity, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            semaphore.Release();
            return entity;
        }

       
    }
}
