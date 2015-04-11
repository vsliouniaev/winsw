using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using winsw;

namespace winswTests
{
    [TestFixture]
    public class CopyStreamTests
    {
        [Test]
        public void TestBufferSizeMultiple()
        {
            Test(40960);
        }

        [Test]
        public void TestDifferentFromBufferSize()
        {
            Test(40961);
        }

        private void Test(int bufferSize)
        {
            var bytes = new byte[bufferSize];
            new Random().NextBytes(bytes);
            var expected = BitConverter.ToString(MD5.Create().ComputeHash(bytes)).Replace("-","");

            var inStr = new MemoryStream(bytes);
            var ouStr = new MemoryStream();

            var actual = Download.CopyStream(inStr, ouStr);

            Assert.AreEqual(expected, actual);
        }
    }
}
