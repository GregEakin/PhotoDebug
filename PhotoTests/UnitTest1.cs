namespace PhotoTests
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib;

    [TestClass]
    public class UnitTest1
    {
        #region Constants

        private const string FileName = @"C:\Users\Greg\Pictures\IMG_0511.CR2";

        #endregion

        #region Public Methods and Operators

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
            {
                throw new ArgumentException("{0} doesn't exists!", FileName);
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
                Assert.AreEqual(0x002A, rawImage.Header.TiffMagic);
                Assert.AreEqual(0x5243, rawImage.Header.CR2Magic);
                CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

                rawImage.DumpHeader(binaryReader);
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var dir = rawImage.Directories.First();
                var length = dir.Length;
                Console.WriteLine("Entries = {0}", length);
                for (var i = 0; i < length; i++)
                {
                    Console.WriteLine(
                        "{0}: {1} {2} {3} {4}",
                        i,
                        dir.Entries[i].TagId,
                        dir.Entries[i].TagType,
                        dir.Entries[i].NumberOfValue,
                        dir.Entries[i].ValuePointer);
                }
                Console.WriteLine("Next offset {0}", dir.NextEntry);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer;
                var strips = directory.Entries.First(e => e.TagId == 0xC640).ValuePointer;

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                while (true)
                {
                    Console.Write("{0,4}h: ", address.ToString("X4"));
                    for (var i = 0; i < 16; i++)
                    {
                        var x = binaryReader.ReadByte();
                        Console.Write("{0,2} ", x.ToString("X2"));
                        address++;
                    }
                    Console.WriteLine();
                    break;
                }
            }
        }

        #endregion
    }
}