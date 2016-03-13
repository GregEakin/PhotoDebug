using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Ifid2Tests
    {
        [TestMethod]
        public void DumpImage0Test()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            DumpImage2(fileName);
        }

        private static void DumpImage2(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Images #0 and #1 are compressed in lossy (classic) JPEG
                var image = rawImage.Directories.Skip(2).First();
                
                Assert.AreEqual(13, image.Entries.Length);
                CollectionAssert.AreEqual(
                    new ushort[]
                    {
                        0x0100, 0x0101, 0x0102, 0x0103, 0x0106, 0x0111, 0x0115, 0x0116, 0x0117, 0x011C, 0xC5D9, 0xC6C5, 0xC6DC
                    },
                    image.Entries.Select(e => e.TagId).ToArray());

                var imageWidth = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(592u, imageWidth);

                var imageHeight = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(395u, imageHeight);

                var imageFileEntry0102 = image.Entries.Single(e => e.TagId == 0x0102 && e.TagType == 3);
                Assert.AreEqual(72014u, imageFileEntry0102.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry0102.NumberOfValue);
                var bitsPerSample = RawImage.ReadUInts16(binaryReader, imageFileEntry0102);
                CollectionAssert.AreEqual(new ushort[] { 16, 16, 16 }, bitsPerSample);

                var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(1u, compression);

                var photometricInterpretation = image.Entries.Single(e => e.TagId == 0x0106 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(2u, photometricInterpretation);

                var stripOffset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(1229532u, stripOffset);

                var samplesPerPixel = image.Entries.Single(e => e.TagId == 0x0115 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(3u, samplesPerPixel);

                var rowsPerStrip = image.Entries.Single(e => e.TagId == 0x0116 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(395u, rowsPerStrip);

                var stripByteCounts = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(1403040u, stripByteCounts);
                Assert.AreEqual(stripByteCounts, imageWidth * imageHeight * samplesPerPixel * 2);

                var planarConfiguration = image.Entries.Single(e => e.TagId == 0x011C && e.TagType == 3).ValuePointer;
                Assert.AreEqual(1u, planarConfiguration);

                // unknown
                var table1 = image.Entries.Single(e => e.TagId == 0xC5D9 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(2u, table1);

                var table2 = image.Entries.Single(e => e.TagId == 0xC6C5 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(3u, table2);

                var imageFileEntryC6DC = image.Entries.Single(e => e.TagId == 0xC6DC && e.TagType == 4);
                // Assert.AreEqual(72020u, imageFileEntry011C.ValuePointer);
                Assert.AreEqual(4u, imageFileEntryC6DC.NumberOfValue);
                var stuff = RawImage.ReadUInts(binaryReader, imageFileEntryC6DC);
                CollectionAssert.AreEqual(new[] { 577u, 386u, 14u, 9u }, stuff);
            }
        }
    }
}
