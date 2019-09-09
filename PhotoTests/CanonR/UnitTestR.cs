// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		UnitTest7D.cs
// AUTHOR:		Greg Eakin

using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.CanonR
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PhotoLib.Tiff;
    using System;
    using System.IO;
    using System.Linq;

    class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        public override short ReadInt16()
        {
            var b = ReadBytes(2);
            return (short) (b[1] + (b[0] << 8));
        }

        public override int ReadInt32()
        {
            var b = ReadBytes(4);
            return b[3] + (b[2] << 8) + (b[1] << 16) + (b[0] << 24);
        }

        public override uint ReadUInt32()
        {
            var b = ReadBytes(4);
            return (uint) b[3] + (uint) (b[2] << 8) + (uint) (b[1] << 16) + (uint) (b[0] << 24);
        }

//        public override long ReadInt64()
//        {
//            this.FillBuffer(8);
//            return (long)(uint)((int)this.m_buffer[4] | (int)this.m_buffer[5] << 8 | (int)this.m_buffer[6] << 16 | (int)this.m_buffer[7] << 24) << 32 | (long)(uint)((int)this.m_buffer[0] | (int)this.m_buffer[1] << 8 | (int)this.m_buffer[2] << 16 | (int)this.m_buffer[3] << 24);
//        }
//
//        public override ulong ReadUInt64()
//        {
//            this.FillBuffer(8);
//            return (ulong)(uint)((int)this.m_buffer[4] | (int)this.m_buffer[5] << 8 | (int)this.m_buffer[6] << 16 | (int)this.m_buffer[7] << 24) << 32 | (ulong)(uint)((int)this.m_buffer[0] | (int)this.m_buffer[1] << 8 | (int)this.m_buffer[2] << 16 | (int)this.m_buffer[3] << 24);
//        }


        public override long ReadInt64()
        {
            var b = ReadBytes(8);
            return (long) b[7] + (b[6] << 8) + (b[5] << 16) + (b[4] << 24) + ((long) b[3] << 32) + ((long) b[2] << 40) +
                   ((long) b[1] << 48) + ((long) b[0] << 56);
        }

        public override ulong ReadUInt64()
        {
            var b = ReadBytes(8);
            return (ulong) b[7] + ((ulong) b[6] << 8) + ((ulong) b[5] << 16) + ((ulong) b[4] << 24) +
                   ((ulong) b[3] << 32) + ((ulong) b[2] << 40) + ((ulong) b[1] << 48) + ((ulong) b[0] << 56);
        }

        /// <summary>Returns <c>true</c> if the Int32 read is not zero, otherwise, <c>false</c>.</summary>
        /// <returns><c>true</c> if the Int32 is not zero, otherwise, <c>false</c>.</returns>
        public bool ReadInt32AsBool()
        {
            var b = ReadBytes(4);
            return b[0] != 0 && b[1] != 0 && b[2] != 0 && b[3] != 0;
        }

        /// <summary>
        /// Reads a string prefixed by a 32-bit integer identifying its length, in chars.
        /// </summary>
        public string ReadString32BitPrefix()
        {
            var length = ReadInt32();
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        public float ReadFloat()
        {
            return (float) ReadDouble();
        }
    }

    // Tags
    //     "ftyp", "moov", "uuid", "stsz", "co64", "PRVW", "CTBO", "CNCV", "CDI1", "IAD1", "CMP1", "CRAW", "THM"

    public class FileTypeBox
    {
        public uint Length { get; }
        public string Type { get; }

        public string MajorBrand { get; }
        public int Version { get; }
        public string[] CompatibleBrands { get; }

        public FileTypeBox(BinaryReader binaryReader)
        {
            // File Type Box
            Length = binaryReader.ReadUInt32();
            Type = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));

            MajorBrand = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
            Version = binaryReader.ReadInt32();

            var tags = new List<string>();
            for (var i = 16; i < Length; i += 4)
            {
                var tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                tags.Add(tag);
            }

            CompatibleBrands = tags.ToArray();
        }
    }

    [TestClass]
    public class UnitTestR
    {
        //private const string FileName = @"D:\Users\Greg\Pictures\EOS R\447A0803.CR3";
        //private const string FileName = @"D:\Users\Greg\Pictures\2019-08-26\8Z4A0057.CR3";
        private const string FileNameRaw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0001.CR3";
        private const string FileNameSraw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0002.CR3";
        private const string FileNameDualRaw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0003.CR3";
        private const string FileNameDualSraw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0004.CR3";
        private const string FileName = FileNameRaw;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
                throw new ArgumentException("{0} doesn't exists!", FileName);

            Console.WriteLine("FileName = {0}", Path.GetFileName(FileName));
            Console.WriteLine("Directory = {0}", Path.GetDirectoryName(FileName));
            Console.WriteLine("FileModifyDate = {0}", File.GetLastWriteTime(FileName));
            Console.WriteLine("FileAccessDate = {0}", File.GetLastAccessTime(FileName));
            Console.WriteLine("FileCreateDate = {0}", File.GetCreationTime(FileName));
        }

        [TestMethod]
        public void RawImageDumpData()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BigEndianBinaryReader(fileStream))
            {
                Console.WriteLine("FileSize {0}", fileStream.Length);

                {
                    var fileTypeBox = new FileTypeBox(binaryReader);
                    Console.WriteLine("Tag: {0}, {1} bytes", fileTypeBox.Type, fileTypeBox.Length);
                    Assert.AreEqual("ftyp", fileTypeBox.Type);

                    Assert.AreEqual("crx ", fileTypeBox.MajorBrand);
                    Assert.AreEqual(1, fileTypeBox.Version);

                    CollectionAssert.AreEquivalent(new[] {"crx ", "isom"}, fileTypeBox.CompatibleBrands);
                }

                {
                    // Container box whose sub-boxes define the metadata for a presentation
                    var length = binaryReader.ReadUInt32();
                    //Assert.AreEqual(0x76D0u, length);

                    var tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                    Console.WriteLine("Tag: {0}, {1} bytes", tag, length);
                    Assert.AreEqual("moov", tag);

                    var data = binaryReader.ReadBytes((int) length - 8);
                    Console.WriteLine("0x{0:X8}, {0} bytes", fileStream.Position);
                }

                {
                    // xpacket data
                    var length = binaryReader.ReadUInt32();
                    //Assert.AreEqual(0x10018u, length);

                    var tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                    Console.WriteLine("Tag: {0}, {1} bytes", tag, length);
                    Assert.AreEqual("uuid", tag);

                    var data = binaryReader.ReadBytes((int) length - 8);
                    // Console.WriteLine("0x{0:X8}, {0} bytes", fileStream.Position);
                }


                {
                    // preview data
                    var length = binaryReader.ReadUInt32();
                    //Assert.AreEqual(0x6AD8Du, length);

                    var tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                    Console.WriteLine("Tag: {0}, {1} bytes", tag, length);
                    Assert.AreEqual("uuid", tag);

                    // PRVW
                    //   jpeg (1620x1080)
                    var data = binaryReader.ReadBytes((int) length - 8);
                    // Console.WriteLine("0x{0:X8}, {0} bytes", fileStream.Position);
                }

                {
                    // main data
                    //   picture #1 (6000x4000, jpeg)
                    //   picture #2 (1624x1080, crx preview)
                    //   picture #3 (6888x4056, crx main image)
                    //   Canon Timed Metadata, CTMD tags below
                    //   picture #5 (6888x4056, dual pixel delta image)

                    var length = binaryReader.ReadUInt32();
                    Assert.AreEqual(0x01u, length);

                    var tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                    Console.WriteLine("Tag: {0}, {1} bytes", tag, length);
                    Assert.AreEqual("mdat", tag);

                    //                    var data = binaryReader.ReadBytes((int)length - 8);
                    Console.WriteLine("0x{0:X8}, {0} bytes", fileStream.Position);
                }

                //var rawImage = new RawImage(binaryReader);
                //CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
                //Assert.AreEqual(0x002A, rawImage.Header.TiffMagic);
                //Assert.AreEqual(0x5243, rawImage.Header.CR2Magic);
                //CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

                //rawImage.DumpHeader(binaryReader);
            }
        }

        public static void ReadFtyp(BinaryReader reader)
        {
        }

        public static void ReadMoov(BinaryReader reader)
        {
        }

        public static void ReadUuid(BinaryReader reader)
        {
        }

        public static void ReadStsz(BinaryReader reader)
        {
        }

        public static void ReadCo64(BinaryReader reader)
        {
        }

        public static void ReadPrvw(BinaryReader reader)
        {
        }

        public static void ReadCtbo(BinaryReader reader)
        {
        }

        public static void ReadThmb(BinaryReader reader)
        {
        }

        public static void ReadCncv(BinaryReader reader)
        {
        }

        public static void ReadCdi1(BinaryReader reader)
        {
        }

        public static void ReadIad1(BinaryReader reader)
        {
        }

        public static void ReadCmp1(BinaryReader reader)
        {
        }

        public static void ReadCraw(BinaryReader reader)
        {
        }

        private readonly Dictionary<string, Action<BinaryReader>> _tags = new Dictionary<string, Action<BinaryReader>>()
        {
            {"ftyp", ReadFtyp},
            {"moov", ReadMoov},
            {"uuid", ReadUuid},
            {"stsz", ReadStsz},
            {"co64", ReadCo64},
            {"prvw", ReadPrvw},
            {"ctbo", ReadCtbo},
            {"thmb", ReadThmb},
            {"cncv", ReadCncv},
            {"cdi1", ReadCdi1},
            {"iad1", ReadIad1},
            {"cmp1", ReadCmp1},
            {"craw", ReadCraw},
        };


        [TestMethod]
        public void ParseCr3Test()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BigEndianBinaryReader(fileStream))
            {
                Console.WriteLine("FileSize {0}", fileStream.Length);

                var length = binaryReader.BaseStream.Length;
                Parse(binaryReader, 0L, length, " ");

                Assert.AreEqual(binaryReader.BaseStream.Length, binaryReader.BaseStream.Position);
            }
        }

        private static void Parse(BigEndianBinaryReader binaryReader, long start, long size, string depth)
        {
            Assert.AreEqual(binaryReader.BaseStream.Position, start);
            // binaryReader.BaseStream.Seek(start, SeekOrigin.Begin);
            while (binaryReader.BaseStream.Position < start + size)
            {
                var s1 = binaryReader.BaseStream.Position;
                var l1 = (long) binaryReader.ReadInt32();
                var chunkName = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                var length = l1 != 1L ? l1 : binaryReader.ReadInt64();
                var b0 = l1 != 1L ? 8L : 16L;

                Console.WriteLine("{0}CN: {1}, size 0x{2:X08}", depth, chunkName, length);

                // if (chunkName not in count) add it +1
                // if (chunkName == "track")
                //   var trackName = "Track {count["trak"]}"

                switch (chunkName)
                {
                    case "ftyp":
                    case "moov":
                    case "uuid":
                    case "stsz":
                    case "co64":
                    case "PRVW":
                    case "CTBO":
                    case "THMB":
                    case "CNCV":
                    case "CDI1":
                    case "IAD1":
                    case "CMP1":
                    case "CRAW":
                        break;
                    case "CMT1":
                    case "CMT2":
                    case "CMT3":
                    case "cmt4":
                        break;
                    case "CTMD":
                        break;
                    default:
                        break;
                }

                switch (chunkName)
                {
                    case "moov":
                    case "trak":
                    case "mdia":
                    case "minf":
                    case "dinf":
                    case "stbl":
                        // Assert.AreEqual(s1 + b0, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0, length - b0, depth + " ");
                        break;
                    case "uuid":
                        var data = binaryReader.ReadBytes(16);
                        var sb = new StringBuilder();
                        for (var i = 0; i < 16; i++)
                            sb.AppendFormat("{0:x2}", data[i]);
                        var uuid1 = sb.ToString();
                        Console.WriteLine("{0}{1}", depth, uuid1);
                        switch (uuid1)
                        {
                            case "85c0b687820f11e08111f4ce462b6a48":
                                // Assert.AreEqual(s1 + b0 + 16, binaryReader.BaseStream.Position);
                                Parse(binaryReader, s1 + b0 + 16, length - b0 - 16, depth + " ");
                                break;
                            case "eaf42b5e1c984b88b9fbb7dc406e4d16":
                                var x = binaryReader.ReadInt64();
                                // Assert.AreEqual(s1 + b0 + 24, binaryReader.BaseStream.Position);
                                Parse(binaryReader, s1 + b0 + 24, length - b0 - 24, depth + " ");
                                break;
                            default:
                                Console.WriteLine("{0}** Unknown UUID {1}, {2} bytes", depth, uuid1, length - b0 - 16);
                                var x1 = binaryReader.ReadBytes((int) (length - b0 - 16));
                                break;
                        }

                        break;
                    case "CRAW":
                        var data1 = binaryReader.ReadBytes(0x52);
                        // Assert.AreEqual(s1 + b0 + 0x52, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0 + 0x52, length - b0 - 0x52, depth + " ");
                        break;
                    case "CCTP":
                        var data2 = binaryReader.ReadBytes(12);
                        // Assert.AreEqual(s1 + b0 + 12, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0 + 12, length - b0 - 12, depth + " ");
                        break;
                    case "stsd":
                        var data3 = binaryReader.ReadBytes(8);
                        // Assert.AreEqual(s1 + b0 + 8, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0 + 8, length - b0 - 8, depth + " ");
                        break;
                    case "dref":
                        var data4 = binaryReader.ReadBytes(8);
                        // Assert.AreEqual(s1 + b0 + 8, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0 + 8, length - b0 - 8, depth + " ");
                        break;
                    case "CTI1":
                        var data5 = binaryReader.ReadBytes(4);
                        // Assert.AreEqual(s1 + b0 + 4, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0 + 4, length - b0 - 4, depth + " ");
                        break;
                    default:
                        Console.WriteLine("{0}** Unknown Chunk {1}, {2} bytes", depth, chunkName, length - b0);
                        var x2 = binaryReader.ReadBytes((int) (length - b0));
                        break;
                }

                //Console.WriteLine("{0}0x{1:X} <= 0x{2:X}, {3}", depth, binaryReader.BaseStream.Position, s1 + length,
                //    -binaryReader.BaseStream.Position + s1 + length);
                Assert.IsTrue(binaryReader.BaseStream.Position <= start + size);
            }

            Console.WriteLine("{0}----", depth);
        }


        [TestMethod]
        public void RawImageSize()
        {
            // 1 Sensor Width                    : 5360 = 1340 * 4 = 2 * 1728 + 1904
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                var strips = directory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer;
                // TIF_CR2_SLICE

                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Assert.AreEqual(2, x);
                Assert.AreEqual(1728, y);
                Assert.AreEqual(1904, z);

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;
                Assert.AreEqual(14, lossless.Precision);
                Assert.AreEqual(4, lossless.Components.Length);
                Assert.AreEqual(1340, lossless.SamplesPerLine);
                Assert.AreEqual(3516, lossless.ScanLines);
            }
        }

        [TestMethod]
        public void Bits()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;
                Assert.AreEqual(14, lossless.Precision);

                var startOfScan = startOfImage.StartOfScan;
                Assert.AreEqual(0, startOfScan.Bb3 & 0x0F);

                Assert.AreEqual(14, lossless.Precision - (startOfScan.Bb3 & 0x0f));
            }
        }

        [TestMethod]
        public void Colors()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;

                Assert.AreEqual(4, lossless.Components.Length); // clrs
                foreach (var component in lossless.Components)
                {
                    Assert.AreEqual(1, component.HFactor); // sraw
                    Assert.AreEqual(1, component.VFactor); // sraw
                }

                Assert.AreEqual(4, lossless.Components.Sum(comp => comp.HFactor * comp.VFactor));
            }
        }

        [TestMethod]
        public void PredictorSelectionValue()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                Assert.AreEqual(1, startOfImage.StartOfScan.Bb1); // Do nothing
            }
        }

        [TestMethod]
        public void DumpRawImageHex()
        {
            // 1 Sensor Width                    : 5360
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                DumpBlock(binaryReader, address, length, 256);

                address = address + length - 64;
                DumpBlock(binaryReader, address, length, 64);
            }
        }

        private static void DumpBlock(BinaryReader binaryReader, uint address, uint length, uint size)
        {
            const int Width = 16;
            binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
            for (var i = 0; i < size; i += Width)
            {
                Console.Write("0x{0:X8}: ", (address + i));
                var nextStep = (int) Math.Min(Width, length - i);
                var data = binaryReader.ReadBytes(nextStep);
                foreach (var b in data)
                {
                    Console.Write("{0:X2} ", b);
                }

                Console.WriteLine();
            }

            Console.WriteLine("...");
        }

        [TestMethod]
        public void TestMethod6()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3)
                    .ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var address =
                    imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117)
                    .ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                Assert.AreEqual(0xFF, startOfImage.Mark);
                Assert.AreEqual(0xD8, startOfImage.Tag); // JPG_MARK_SOI

                var huffmanTable = startOfImage.HuffmanTable;
                Assert.AreEqual(0xFF, huffmanTable.Mark);
                Assert.AreEqual(0xC4, huffmanTable.Tag);

                // This file has two huffman tables: 0x00 and 0x01
                Assert.AreEqual(2, huffmanTable.Tables.Count);
                Assert.IsTrue(huffmanTable.Tables.ContainsKey(0x00));
                Assert.IsTrue(huffmanTable.Tables.ContainsKey(0x01));

                var lossless = startOfImage.StartOfFrame;
                Assert.AreEqual(0xFF, lossless.Mark);
                Assert.AreEqual(0xC3, lossless.Tag);

                Assert.AreEqual(14, lossless.Precision);
                Assert.AreEqual(4, lossless.Components.Length);
                Assert.AreEqual(1340, lossless.SamplesPerLine);
                Assert.AreEqual(3516, lossless.ScanLines);

                Assert.AreEqual(5360, lossless.Width); // Sensor width (bits)
                Assert.AreEqual(5360, lossless.SamplesPerLine * lossless.Components.Length);
                Assert.AreEqual(5360, x * y + z);

                foreach (var component in lossless.Components)
                {
                    // Console.WriteLine("== {0}: {1} {2} {3}", component.ComponentId, component.HFactor, component.VFactor, component.TableId);
                    Assert.AreEqual(0x01, component.HFactor);
                    Assert.AreEqual(0x01, component.VFactor);
                    Assert.AreEqual(0x00, component.TableId);
                }

                var startOfScan = startOfImage.StartOfScan;
                Assert.AreEqual(0xFF, startOfScan.Mark);
                Assert.AreEqual(0xDA, startOfScan.Tag);

                foreach (var scanComponent in startOfScan.Components)
                {
                    Console.WriteLine("{0}: {1} {2}", scanComponent.Id, scanComponent.Dc, scanComponent.Ac);
                }

                var imageData = startOfImage.ImageData;
            }
        }
    }
}