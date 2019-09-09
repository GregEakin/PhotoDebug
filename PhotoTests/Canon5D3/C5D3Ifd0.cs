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

namespace PhotoTests.Canon5D3
{
    [TestClass]
    public class C5D3Ifd0
    {
        //private const string FileName = @"d:\Users\Greg\Pictures\2018-08-29\0L2A3743.CR2";
        private const string FileName = @"d:\Users\Greg\Pictures\2019-08-18\0L2A4564.CR2";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Assert.IsTrue(File.Exists(FileName), "Image file {0} doesn't exists!", FileName);
        }

        [TestMethod]
        public void DumpImageFileDirectory()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.First();
                imageFileDirectory.DumpDirectory(binaryReader);
            }
        }

        [TestMethod]
        public void ReadImageGuid()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var ifid0 = rawImage.Directories.First();
                var exif = ifid0[0x8769]; // Exif Offset
                binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);

                var makerNotes = tags[0x927C]; // Maker Notes
                binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                var settings = notes.Entries[0x0028]; // ImageUniqueID
                if (settings == null) return;

                // notes.DumpDirectory(binaryReader);
                // 0L2A3742.CR2: 0x4027 40)  ULong 32-bit: [0x00010F38] (5): 00000014 00090207 A9A50034 073C0600 00D0D0D0 
                // 0L2A3743.CR2: 0x4027 40)  ULong 32-bit: [0x00010F38] (5): 00000014 00090207 A9A50034 073C0600 00D0D0D0 
                // 0L2A3744.CR2: 0x4027 40)  ULong 32-bit: [0x00010F38] (5): 00000014 00090207 A9A50034 073C0600 00D0D0D0

                // 20 590343 2846162996 121374208 13684944      <-- my camera
                // 20 395523 2509701170 121373952 13684944      <-- http://russellharrison.com/bugs/darktable/5D_Mk3/IT8-0315.txt
                // 24 524547 2969567282 134217728 13684944 0    <-- https://github.com/drewnoakes/metadata-extractor-images/blob/master/cr2/metadata/dotnet/Canon%20EOS%2070D.cr2.txt

                binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
                var settingsData = new uint[settings.NumberOfValue];
                for (var i = 0; i < settings.NumberOfValue; i++)
                    settingsData[i] = binaryReader.ReadUInt32();

                Console.WriteLine("addr 0x{0:x8}", settings.ValuePointer);
                Console.WriteLine("size {0}", settings.NumberOfValue);
                for (var i = 0; i < settings.NumberOfValue; i++)
                    Console.Write("{0:x8} ", settingsData[i]);
                Console.WriteLine();
                for (var i = 0; i < settings.NumberOfValue; i++)
                    Console.Write("{0} ", settingsData[i]);
                Console.WriteLine();

                binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
                int size = binaryReader.ReadInt32() - 4;
                var data = binaryReader.ReadBytes(size);
                var guid = new Guid(data);
                Console.WriteLine("Guid = {0}", guid); // Guid = c261806b-f82c-9003-427a-06be63189acb

//                binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
//                int size1 = binaryReader.ReadInt32() - 4;
//                int addr1 = binaryReader.ReadInt32();
//                binaryReader.BaseStream.Seek(addr1, SeekOrigin.Begin);
//                var xx = binaryReader.ReadBytes(16);
//                var guid2 = new Guid(xx);
//                Console.WriteLine("Guid = {0}", guid2); // Guid = c261806b-f82c-9003-427a-06be63189acb
//                for (var i = 0; i < xx.Length; i++)
//                    Console.Write("{0:x2} ", xx[i]);

                //addr 0x00010f38
                //size 5
                //00000014 00090207 a9a50034 073c0600 00d0d0d0
                //20 590343 2846162996 121374208 13684944
                //Guid = 00090207 - 0034 - a9a5 - 0006 - 3c07d0d0d000
                //Guid = b87795a5 - 6a21 - aca4 - 7885 - f8ac8247f2e3
                //a5 95 77 b8 21 6a a4 ac 78 85 f8 ac 82 47 f2 e3
                
                //addr 0x00010f38
                //size 5
                //00000014 00090207 a9a50034 073c0600 00d0d0d0
                //20 590343 2846162996 121374208 13684944
                //Guid = 00090207 - 0034 - a9a5 - 0006 - 3c07d0d0d000
                //Guid = e0ca2963 - a705 - acc6 - 81c9 - ce49e83bd6a9
                //63 29 ca e0 05 a7 c6 ac 81 c9 ce 49 e8 3b d6 a9
            }
        }

        [TestMethod]
        public void ImageWidth()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0100 UShort 16-bit: 5760
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0100];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(5760u, imageFileEntry.ValuePointer);
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

                // 0x0101 UShort 16-bit: 3840
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0101];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(3840u, imageFileEntry.ValuePointer);
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

                // 0x0102 UShort 16-bit: [0x000000EE] (3): 8, 8, 8, 
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0102];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(238u, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] {(ushort) 8, (ushort) 8, (ushort) 8},
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

                // 0x010F Ascii 8-bit: [0x000000F4] (6): Canon
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x010F];
                Assert.AreEqual(2, imageFileEntry.TagType);
                Assert.AreEqual(0x000000F4u, imageFileEntry.ValuePointer);
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

                // 0x0110 Ascii 8-bit: [0x000000FA] (22): Canon EOS 5D Mark III
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x0110];
                Assert.AreEqual(2, imageFileEntry.TagType);
                Assert.AreEqual(0x000000FAu, imageFileEntry.ValuePointer);
                Assert.AreEqual(22u, imageFileEntry.NumberOfValue);

                Assert.AreEqual("Canon EOS 5D Mark III", RawImage.ReadChars(binaryReader, imageFileEntry));
            }
        }

        // stripOffset 6)  0x0111 ULong 32-bit: 96332
        // orientation 7)  0x0112 UShort 16-bit: 1
        // stripByteCounts 8)  0x0117 ULong 32-bit: 2390306
        // xResolution 9)  0x011A Rational 2x32-bit: [0x0000011A] (2): 72/1 = 72
        // yResolution 10)  0x011B Rational 2x32-bit: [0x00000122] (2): 72/1 = 72
        // resolutionUnit 11)  0x0128 UShort 16-bit: 2, pixels per inch
        // dateTime 12)  0x0132 Ascii 8-bit: [0x0000012A] (20): 2013:07:13 01:10:00
        // 13)  0x013B Ascii 8-bit: [0x0000013E] (11): Greg Eakin

        [TestMethod]
        public void XmpMetadata()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x02BC Byte 8-bit: [0x000119C4] (8192): // XML packet containing XMP metadata
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x02BC];
                Assert.AreEqual(1, imageFileEntry.TagType);
                Assert.AreEqual(0x000119C4u, imageFileEntry.ValuePointer);
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

                // 0x8769 Image File Directory: [0x000001BE] (1): 
                var imageFileDirectory = rawImage.Directories.First();
                var imageFileEntry = imageFileDirectory[0x8769];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x000001BEu, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);

                var readULongs = RawImage.ReadUInts(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new[] {0x829a0026}, readULongs);
            }
        }

        // 6)  0x0111 ULong 32-bit: 91648u     -- Offset
        // 8)  0x0117 ULong 32-bit: 2702898u   -- Length
        [TestMethod]
        public void DumpImage0()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.First();

                var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(91648u, offset);

                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(2702898u, length);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                var dir = Path.GetDirectoryName(FileName) ?? ".";
                var name = Path.GetFileNameWithoutExtension(FileName) + "-0.jpg";
                var path = Path.Combine(dir, name);
                DumpImage(path, binaryReader, length);
            }
        }

        private static void DumpImage(string filename, BinaryReader binaryReader, uint length)
        {
            using (var stream = File.Create(filename))
            {
                var bytes = (int) length;
                var buffer = new byte[32768];
                int read;
                while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) >
                       0)
                {
                    stream.Write(buffer, 0, read);
                    bytes -= read;
                }
            }
        }
    }
}