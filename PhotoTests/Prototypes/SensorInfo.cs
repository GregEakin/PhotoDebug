using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class SensorInfo
    {
        [TestMethod]
        public void DumpSensorInfo()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image[0x8769];
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif[0x927C];
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                var sensorInfo = notes[0x00E0];
                var stuff = RawImage.ReadUInts16(binaryReader, sensorInfo);
                CollectionAssert.AreEqual(new[]
                {
                    (ushort)34u,     // length
                    (ushort)5920u,   // sensor width
                    (ushort)3950u,   // sensor height
                    (ushort)1u,      // sensor left
                    (ushort)1u,      // sensor top
                    (ushort)140u,    // sensor right
                    (ushort)96u,     // sensor bottom
                    (ushort)5899u,
                    (ushort)3935u,
                    (ushort)0u,(ushort)0u,(ushort)0u,(ushort)0u,
                    (ushort)0u,(ushort)0u,(ushort)0u,(ushort)0u
                }, stuff);
            }
        }

        [TestMethod]
        public void DumpCensorInfo2()
        {
            const string fileName = @"C:..\..\Photos\7DSraw.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image[0x8769];
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif[0x927c];
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                var sensorInfo = notes[0x00E0];
                var stuff = RawImage.ReadUInts16(binaryReader, sensorInfo);
                CollectionAssert.AreEqual(new[]
                {
                    (ushort)34u,     // length
                    (ushort)2592u,   // sensor width
                    (ushort)1728u,   // sensor height
                    (ushort)1u,      // sensor left
                    (ushort)1u,      // sensor top
                    (ushort)0u,    // sensor right
                    (ushort)0u,     // sensor bottom
                    (ushort)2591u,
                    (ushort)1727u,
                    (ushort)0u,(ushort)0u,(ushort)0u,(ushort)0u,
                    (ushort)0u,(ushort)0u,(ushort)0u,(ushort)0u
                }, stuff);
            }
        }

        [TestMethod]
        public void DumpSensorInfo3()
        {
            const string fileName = @"C:..\..\Photos\7Dhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image[0x8769];
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif[0x927c];
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                var sensorInfo = notes[0x00E0];
                var stuff = RawImage.ReadUInts16(binaryReader, sensorInfo);
                CollectionAssert.AreEqual(new[]
                {
                    (ushort)34u,     // length
                    (ushort)5360u,   // sensor width
                    (ushort)3516u,   // sensor height
                    (ushort)1u,      // sensor left
                    (ushort)1u,      // sensor top
                    (ushort)168u,    // sensor right
                    (ushort)56u,     // sensor bottom
                    (ushort)5351u,
                    (ushort)3511u,
                    (ushort)0u,(ushort)0u,(ushort)0u,(ushort)0u,
                    (ushort)0u,(ushort)0u,(ushort)0u,(ushort)0u
                }, stuff);
            }
        }
    }
}
