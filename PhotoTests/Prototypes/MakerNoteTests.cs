using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class MakerNoteTests
    {
        [TestMethod]
        public void DumpMakerNotes1()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // camera settings notes[0x0001]
                // focus info notes[0x0002]
                // image type notes[0x0006]
                // dust delete notes[0x0097]
                // sensor info notes[0x00E0]
                // color balance notes[0x4001]
                // AF Micro adjust notes[0x4013]
                // Vignetting correction notes[0x40015]

                var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                Assert.AreEqual("Canon EOS 5D Mark III", model);

                var firmware = RawImage.ReadChars(binaryReader, notes[0x0007]);
                Assert.AreEqual("Firmware Version 1.2.3\0", firmware);

                // 0x0010 ULong 32 - bit: 2147484293
                var id = notes.Entries.Single(e => e.TagId == 0x0010 && e.TagType == 4);
                Assert.AreEqual(0x80000285, id.ValuePointer);
            }
        }

        [TestMethod]
        public void DumpMakerNotes2()
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

                var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                Assert.AreEqual("Canon EOS 7D", model);

                var firmware = RawImage.ReadChars(binaryReader, notes[0x0007]);
                Assert.AreEqual("Firmware Version 2.0.5\0", firmware);

                // 0x0010 ULong 32 - bit: 2147484240
                var id = notes[0x0010];
                Assert.AreEqual(0x80000250, id.ValuePointer);
            }
        }

        [TestMethod]
        public void DumpMakerNotes3()
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

                var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                Assert.AreEqual("Canon EOS 7D", model);

                var firmware = RawImage.ReadChars(binaryReader, notes[0x0007]);
                Assert.AreEqual("Firmware Version 2.0.3\0", firmware);

                // 0x0010 ULong 32 - bit: 2147484240
                var id = notes[0x0010];
                Assert.AreEqual(0x80000250, id.ValuePointer);
            }
        }
    }
}
