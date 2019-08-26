// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.CanonM5
{
    // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
    // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
    // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance, correction has been applied.
    // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

    [TestClass]
    public class CM5Ifd0
    {
        // private const string FileName = @"C:..\..\..\Samples\IMG_0012.CR2";
        private const string FileName = @"D:\Users\Greg\Pictures\2017-11-21\IMG_0002.CR2";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
                throw new ArgumentException("{0} doesn't exists!", FileName);
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
                Assert.AreEqual(0x002A, rawImage.Header.TiffMagic);
                Assert.AreEqual(0x5243, rawImage.Header.CR2Magic);
                CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

                rawImage.DumpHeader(binaryReader);
            }
        }

        [TestMethod]
        public void ReadImageGuid()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 15)  0x0028 UByte 8-bit: [0x00003618] (16): 6b, 80, 61, c2, 2c, f8, 03, 90, 42, 7a, 06, be, 63, 18, 9a, cb, 
                var ifid0 = rawImage.Directories.First();
                var exif = ifid0[0x8769];   // Exif Offset
                binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);

                var makerNotes = tags[0x927C];  // Maker Notes
                binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);
                var settings = notes[0x0028];
                binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
                var settingsData = new byte[settings.NumberOfValue];
                for (var i = 0; i < settings.NumberOfValue; i++)
                    settingsData[i] = binaryReader.ReadByte();

                var guid = new Guid(settingsData);
                Console.WriteLine("Guid = {0}", guid);  // Guid = c261806b-f82c-9003-427a-06be63189acb
            }
        }
        
        [Ignore]
        [TestMethod]
        public void DumpGuids()
        {
            var d = new DirectoryInfo(@"D:\Users\Greg\Pictures\2017-11-21");
            var files = d.GetFiles("*.cr2");
            foreach (var file in files)
            {
                using (var fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read))
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    var rawImage = new RawImage(binaryReader);

                    // 15)  0x0028 UByte 8-bit: [0x00003618] (16): 6b, 80, 61, c2, 2c, f8, 03, 90, 42, 7a, 06, be, 63, 18, 9a, cb, 
                    var ifid0 = rawImage.Directories.First();
                    var exif = ifid0[0x8769];   // Exif Offset
                    binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);

                    var makerNotes = tags[0x927C];  // Maker Notes
                    binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
                    var notes = new ImageFileDirectory(binaryReader);
                    var settings = notes[0x0028];
                    binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
                    var settingsData = new byte[settings.NumberOfValue];
                    for (var i = 0; i < settings.NumberOfValue; i++)
                        settingsData[i] = binaryReader.ReadByte();

                    var guid = new Guid(settingsData);
                    Console.WriteLine("Guid = {0}  File = {1}", guid, file.Name);  // Guid = c261806b-f82c-9003-427a-06be63189acb
                }
            }
        }


        [TestMethod]
        public void ImageWidth()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0100 UShort 16-bit: 6000
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0100];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(6000u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void ImageLength()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0101 UShort 16-bit: 4000
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0101];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(4000u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void BitsPerSample()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0102 UShort 16-bit: [0x000000FA] (3): 8, 8, 8, 
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0102];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(250u, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] { (ushort)8, (ushort)8, (ushort)8 },
                    RawImage.ReadUInts16(binaryReader, imageFileEntry));
            }
        }

        [TestMethod]
        public void Compression()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0103 UShort 16-bit: 6
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0103];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(6u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void Maker()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x010F Ascii 8-bit: [0x00000120] (6): Canon
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x010F];
                Assert.AreEqual(2, imageFileEntry.TagType);
                Assert.AreEqual(0x00000120u, imageFileEntry.ValuePointer);
                Assert.AreEqual(6u, imageFileEntry.NumberOfValue);

                Assert.AreEqual("Canon", RawImage.ReadChars(binaryReader, imageFileEntry));
            }
        }

        [TestMethod]
        public void Model()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0110 Ascii 8-bit: [0x00000126] (22): Canon EOS M5
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0110];
                Assert.AreEqual(2, imageFileEntry.TagType);
                Assert.AreEqual(0x00000126u, imageFileEntry.ValuePointer);
                Assert.AreEqual(13u, imageFileEntry.NumberOfValue);

                Assert.AreEqual("Canon EOS M5", RawImage.ReadChars(binaryReader, imageFileEntry));
            }
        }

        // 7)  0x0111 ULong 32-bit: 73728   - Strip offset
        // 8)  0x0112 UShort 16-bit: 1      - Orientation
        // 9)  0x0117 ULong 32-bit: 6590185 - Strib Byte Counts
        // 10)  0x011A URational 2x32-bit: [0x00000134] (1): 180/1 = 180    - X Resolution
        // 11)  0x011B URational 2x32-bit: [0x0000013C] (1): 180/1 = 180    - Y Resolution
        // 12)  0x0128 UShort 16-bit: 2                - Resolution Units, pixels per inch
        // 13)  0x0132 Ascii 8-bit, null terminated: [0x00000144] (20): "2017:10:14 22:59:10"   // Date Time
        // 14)  0x013B Ascii 8-bit, null terminated: [0x0BFF0000] (1): ""                       // Owner

        [TestMethod]
        public void XmpMetadata()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 15)  0x02BC UByte 8-bit: [0x0000BC00] (8192): // XML packet containing XMP metadata
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x02BC];
                Assert.AreEqual(1, imageFileEntry.TagType);
                Assert.AreEqual(0x0000BC00u, imageFileEntry.ValuePointer);
                Assert.AreEqual(8192u, imageFileEntry.NumberOfValue);

                var readChars = RawImage.ReadChars(binaryReader, imageFileEntry);

                const string expected1 =
                    "<?xpacket begin='ï»¿' id='W5M0MpCehiHzreSzNTczkc9d'?><x:xmpmeta xmlns:x=\"adobe:ns:meta/\"><rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description rdf:about=\"\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\"><xmp:Rating>0</xmp:Rating></rdf:Description></rdf:RDF></x:xmpmeta>";
                Assert.AreEqual(expected1, readChars.Substring(0, 291));

                // lots of white space between these two substrings.
                Assert.IsTrue(string.IsNullOrWhiteSpace(readChars.Substring(291, 8173 - 291)));

                const string expected2 = "<?xpacket end='w'?>";
                Assert.AreEqual(expected2, readChars.Substring(8173));
            }
        }

        // 15)  0x8298 Ascii 8-bit: [0x0000017E] (11): Greg Eakin

        [TestMethod]
        public void ExifTags()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x8769 Image File Directory: [0x000001D8] (1): 
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x8769];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x000001D8u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);

                var readULongs = RawImage.ReadUInts(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new[] { 0x829a002a }, readULongs);
            }
        }

        // 7)  0x0111 ULong 32-bit: 73728u     -- Offset
        // 9)  0x0117 ULong 32-bit: 6590185u   -- Length
        [TestMethod]
        public void DumpImage()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.First();

                var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(73728u, offset);

                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(6590185u, length);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var name = Path.Combine(Path.GetDirectoryName(FileName) ?? "./", Path.GetFileNameWithoutExtension(FileName) + "-0.jpg");
                DumpImage(name, binaryReader, length);
            }
        }

        private static void DumpImage(string output, BinaryReader binaryReader, uint length)
        {
            using (var x = File.Create(output))
            {
                var bytes = (int)length;
                var buffer = new byte[32768];
                int read;
                while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
                {
                    x.Write(buffer, 0, read);
                    bytes -= read;
                }
            }
        }

        /// <summary>
        /// Convert Canon hex-based EV (modulo 0x20) to real number
        /// Inputs: 0) value to convert
        /// ie) 0x00 -> 0
        ///     0x0c -> 0.33333
        ///     0x10 -> 0.5
        ///     0x14 -> 0.66666
        ///     0x20 -> 1   ...  etc
        /// </summary>
        private static float CanonEv(int ev)
        {
            return ev / 32f;
        }

        [TestMethod]
        public void TestEv1()
        {
            Assert.AreEqual(-1.0f, CanonEv(-0x20));
            //Assert.AreEqual(-2f / 3f, CanonEv(-0x14));
            Assert.AreEqual(-1f / 2f, CanonEv(-0x10));
            //Assert.AreEqual(-1f / 3f, CanonEv(-0x0c));
            Assert.AreEqual(0.0f, CanonEv(0x00));
            //Assert.AreEqual(1f / 3f, CanonEv(0x0c));
            Assert.AreEqual(1f / 2f, CanonEv(0x10));
            //Assert.AreEqual(2f / 3f, CanonEv(0x14));
            Assert.AreEqual(1.0f, CanonEv(0x20));
        }
    }
}