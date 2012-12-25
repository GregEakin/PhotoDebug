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

        [TestInitialize]
        public void Initialize()
        {
            if (!File.Exists(FileName))
            {
                throw new ArgumentException("{0} doesn't exists!", FileName);
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    var rawImage = new RawImage(binaryReader);
                    CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
                    Assert.AreEqual(0x002A, rawImage.Header.TiffMagic);
                    Assert.AreEqual(0x5243, rawImage.Header.CR2Magic);
                    CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);
                }
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
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
        }

        #endregion
    }
}