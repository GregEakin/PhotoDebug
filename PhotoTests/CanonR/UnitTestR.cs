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
    using System;
    using System.IO;

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
        public void DirectDumpTest()
        {
            using (var fileStream = File.Open(FileNameRaw, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BigEndianBinaryReader(fileStream))
            {
                Console.WriteLine("FileSize {0}", fileStream.Length);

                // CTBO[1]
                binaryReader.BaseStream.Seek(0x00007260 + 0x18, SeekOrigin.Begin);
                var xPacket = binaryReader.ReadBytes(0x00010018 - 0x18);
                File.WriteAllBytes("xpacket.txt", xPacket);

                // CTBO[2]
                binaryReader.BaseStream.Seek(0x00017278 + 0x38, SeekOrigin.Begin);
                var preview = binaryReader.ReadBytes(0x00045E0D - 0x38);
                File.WriteAllBytes("preview.jpg", preview);

                // mdat (main data)
                //var fp3 = new byte[0x024EE898];
                //Buffer.BlockCopy(y21, 0x0005D085, fp3, 0, 0x024EE898); (16 bytes too much)
                // note: buffer size is: 38725768 bytes 
            }
        }

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
            var tagCount = new Dictionary<string, int>();
            var cr3 = new Dictionary<string, Dictionary<string, int>>();

            Assert.AreEqual(binaryReader.BaseStream.Position, start);
            // binaryReader.BaseStream.Seek(start, SeekOrigin.Begin);
            while (binaryReader.BaseStream.Position < start + size)
            {
                var s1 = binaryReader.BaseStream.Position;
                var l1 = (long) binaryReader.ReadInt32();
                var chunkName = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                var length = l1 != 1L ? l1 : binaryReader.ReadInt64();
                var b0 = l1 != 1L ? 8L : 16L;

                switch (chunkName)
                {
                    case "free":
                        var xf = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "ftyp": // File Type Box
                        var majorBrand = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                        var version = binaryReader.ReadInt32();

                        var tags = new List<string>();
                        for (var i = 16; i < length; i += 4)
                        {
                            var tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                            tags.Add(tag);
                        }

                        // var compatibleBrands = tags.ToArray();
                        Console.Write("{0}ftyp: {1} {2}: ", depth, majorBrand, version);
                        foreach (var tag in tags) Console.Write("{0}, ", tag);
                        Console.WriteLine();
                        break;

                    case "moov": // Container box whose sub-boxes define the metadata for presentation
                        Console.WriteLine("{0}moov:", depth);
                        Parse(binaryReader, s1 + b0, length - b0, depth + " ");
                        break;

                    case "CNCV": // Canon Compressor Version
                        var y11 = binaryReader.ReadBytes((int) (length - b0));
                        var x11 = Encoding.ASCII.GetString(y11);
                        Console.WriteLine("{0}CNCV: {1}", depth, x11);
                        // if (CRM) 
                        //   video = data[ cr3["CTBO"][3][0] + 0x50 : cr3['CTBO'][3][0] + cr3['CTBO'][3[1] - 0x50]
                        // if (CR3) Console.WriteLine("extract JPEG (track1) {0}x{1} from mdat: offset - 0x{2:X}, size = 0x{3:X}", 
                        //              cr3["trak1"]["CRAW"][0], cr3["trak1"]["CRAW"][1], cr3["trak1"]["co64"], cr3["trak1"]["stsz"]); 
                        //     jpeg = trakData("trak1")
                        //     File.WriteAllBytes("trak1.jpg", jpeg);
                        break;

                    case "CCTP": // Canon CR3 Track Pointers
                        var cctp0 = binaryReader.ReadInt32();
                        var cctp1 = binaryReader.ReadInt32();
                        var cctp2 = binaryReader.ReadInt32(); // number of CCDT lines: 3, or 4 for dual pixel
                        Console.WriteLine("{0}CCTP: {1}, {2}, {3}", depth, cctp0, cctp1, cctp2);
                        Parse(binaryReader, s1 + b0 + 12, length - b0 - 12, depth + " ");
                        break;

                    case "CCDT": // Canon Compressor Data Type
                        var ccdt0 = binaryReader.ReadInt64();
                        var ccdt1 = binaryReader.ReadInt32();
                        var ccdt2 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}CCDT type {1}, {2}, {3}", depth, ccdt0, ccdt1, ccdt2);
                        break;

                    case "CTBO": // Canon Trak B Offsets
                        var l9 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}CTBO: {1}", depth, l9);
                        for (var i9 = 0; i9 < l9; i9++)
                        {
                            var idx9 = binaryReader.ReadInt32();
                            var offset9 = binaryReader.ReadInt64();
                            var size9 = binaryReader.ReadInt64();
                            Console.WriteLine("{0}  {1}, 0x{2:X8}, 0x{3:X8}", depth, idx9, offset9, size9);
                        }

                        break;

                    case "CMT1":
                    case "CMT2":
                    case "CMT3":
                    case "CMT4":
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y12 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "THMB": // Thumbnail
                        var a8 = binaryReader.ReadInt32(); // 4    == 0x08 - 8 = 0x00
                        var w8 = binaryReader.ReadInt16(); // 2    == 0x0C - 8 = 0x04
                        var h8 = binaryReader.ReadInt16(); // 2    == 0x0E - 8 = 0x06
                        var s8 = binaryReader.ReadInt32(); // 4    == 0x10 - 8 = 0x08
                        var m8 = binaryReader.ReadInt16(); // 4    == 0x14 - 8 = 0x0C
                        var n8 = binaryReader.ReadInt16(); // 4    == 0x14 - 8 = 0x0C
                        var b8 = binaryReader.ReadBytes(s8); // s8 == 0x18 - 8 = 0x10
                        File.WriteAllBytes("thmb.jpg", b8);

                        var pad8 = length - b0 - 16L - s8;
                        var p8 = binaryReader.ReadBytes((int) pad8);

                        Console.WriteLine("{0}THMB: {1}, {2}, 0x{3:X}, a={4}, m={5}, n={6}, {7} byte pad", depth, w8,
                            h8, s8, a8, m8, n8, pad8);
                        //foreach (var bp in p8) Console.Write("{0:X} ", bp);
                        //Console.WriteLine();
                        Assert.AreEqual(length, b0 + 16L + s8 + pad8);
                        Assert.AreEqual(binaryReader.BaseStream.Position, s1 + b0 + 16L + s8 + pad8);
                        break;

                    case "mvhd": // Movie Header
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y15 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "trak": // Track: Embedded JPEG
                        var cc = tagCount.TryGetValue(chunkName, out var vv);
                        tagCount[chunkName] = cc ? vv + 1 : 1;

                        var trakName = $"trak{tagCount["trak"]}";
                        if (!cr3.ContainsKey(trakName))
                            cr3[trakName] = new Dictionary<string, int>();

                        string trakType;
                        switch (trakName)
                        {
                            case "trak1":
                                trakType = "full size jpeg image: 6000x4000";
                                break;
                            case "trak2":
                                trakType = "Small definition raw image: 1624x1080";
                                break;
                            case "trak3":
                                trakType = "High definition raw image: 6888x4546";
                                break;
                            case "trak4":
                                trakType = "Canon Timed Metadata";
                                break;
                            case "trak5":
                                trakType = "Dual pixel delta track: 6888x4546";
                                break;
                            default:
                                trakType = "Unknown";
                                break;
                        }

                        Console.WriteLine("{0}{1}: {2}", depth, trakName, trakType);
                        break;

                    case "tkhd": // Track Header
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y13 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "mdia": // Media
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        Parse(binaryReader, s1 + b0, length - b0, depth + " ");
                        break;

                    case "mdhd": // Media Header
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y16 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "hdlr": // Handler, type = "vide"
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y14 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "minf": // Media Information Container
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        Parse(binaryReader, s1 + b0, length - b0, depth + " ");
                        break;

                    case "vmhd": // Video Media Header
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y7 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "dinf": // Data Information Box
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        Parse(binaryReader, s1 + b0, length - b0, depth + " ");
                        break;

                    case "dref": // Data Reference Box
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        var data4 = binaryReader.ReadBytes(8);
                        Parse(binaryReader, s1 + b0 + 8, length - b0 - 8, depth + " ");
                        break;

                    case "stbl": // Sample Table Box
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        Parse(binaryReader, s1 + b0, length - b0, depth + " ");
                        break;

                    case "stsd": // Samble Descriptions, Codex Types, Init, etc.
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        var data3 = binaryReader.ReadBytes(8);
                        Parse(binaryReader, s1 + b0 + 8, length - b0 - 8, depth + " ");
                        break;

                    case "CRAW": // Lossy Compression
                        Console.WriteLine("{0}{1}, size 0x{2:X08}", depth, chunkName, length);
                        var data1 = binaryReader.ReadBytes(0x52);
                        Parse(binaryReader, s1 + b0 + 0x52, length - b0 - 0x52, depth + " ");
                        break;

                    case "JPEG":
                        var a5 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}JPEG: {1}", depth, a5);
                        break;

                    case "stts": // Decoding, Time To Sample
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y17 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "stsc": // Sample To Chuck, partial data offset info
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y18 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "stsz": // Sample Size, Framing, size of jpeg picture #1 in mdat
                        var v2 = binaryReader.ReadInt32();
                        var s2 = binaryReader.ReadInt32();
                        var c2 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}stsz: {1}, {2}, {3}", depth, v2, s2, c2);
                        break;

                    case "co64": // Pointer to Picture #1 inside mdat
                        var v3 = binaryReader.ReadInt32();
                        var s3 = binaryReader.ReadInt32();
                        var c3 = binaryReader.ReadInt32();
                        var d3 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}co64: {1}, {2}, {3}, {4}", depth, v3, s3, c3, d3);
                        break;

                    case "mdat": // Main Data
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y21 = binaryReader.ReadBytes((int)(length - b0));

                        // xpacket uuid
                        //var fp1 = new byte[0x00010018];
                        //Buffer.BlockCopy(y21, 0x00007260, fp1, 0, 0x00010018);

                        // preview uuid
                        //var fp2 = new byte[0x00045E0D];
                        //Buffer.BlockCopy(y21, 0x00017278, fp2, 0, 0x00045E0D);
                        //File.WriteAllBytes("p2.jpg", fp2);

                        // mdat (main data)
                        //var fp3 = new byte[0x024EE898];
                        //Buffer.BlockCopy(y21, 0x0005D085, fp3, 0, 0x024EE898); (16 bytes too much)
                        // note: buffer size is: 38725768 bytes 

                        break;


                    /////////////


                    case "PRVW": // Preview
                        var a7 = binaryReader.ReadInt32(); // 4    == 0x08 - 8 = 0x00
                        var m7 = binaryReader.ReadInt16(); // 2    == 0x0C - 8 = 0x04
                        var w7 = binaryReader.ReadInt16(); // 2    == 0x0E - 8 = 0x06
                        var h7 = binaryReader.ReadInt16(); // 2    == 0x10 - 8 = 0x08
                        var n7 = binaryReader.ReadInt16(); // 2    == 0x12 - 8 = 0x0A
                        var s7 = binaryReader.ReadInt32(); // 4    == 0x14 - 8 = 0x0C
                        var b7 = binaryReader.ReadBytes(s7); // s7   == 0x18 - 8 = 0x10
                        File.WriteAllBytes("prvw.jpg", b7);

                        Console.WriteLine("{0}PRVW: {1}, {2}, 0x{3:X}, {4}, {5}", depth, w7, h7, s7, m7, n7);
                        Assert.AreEqual(length, b0 + 16 + s7);
                        Assert.AreEqual(binaryReader.BaseStream.Position, s1 + b0 + 16L + s7);
                        break;

                    case "CDI1":
                        Console.WriteLine("{0}CDI1: {1} bytes", depth, length - b0);
                        var pad10 = length - b0;
                        var p10 = binaryReader.ReadBytes((int) pad10);
                        break;

                    case "url ":
                        var a4 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}url : {1}", depth, a4);
                        break;

                    case "nmhd":
                        var a6 = binaryReader.ReadInt32();
                        Console.WriteLine("{0}nmhd: {1}", depth, a6);
                        break;

                    case "uuid":
                        var data = binaryReader.ReadBytes(16);
                        var sb = new StringBuilder();
                        for (var i = 0; i < 16; i++)
                            sb.AppendFormat("{0:x2}", data[i]);
                        var uuid1 = sb.ToString();
                        Console.WriteLine("{0}uuid: {1}", depth, uuid1);
                        switch (uuid1)
                        {
                            case "85c0b687820f11e08111f4ce462b6a48":
                                Parse(binaryReader, s1 + b0 + 16, length - b0 - 16, depth + " ");
                                break;
                            case "be7acfcb97a942e89c71999491e3afac": // xpacket data
                                var x4c = binaryReader.ReadBytes((int) (length - b0 - 16));
                                var x4text = Encoding.ASCII.GetString(x4c);
                                Assert.IsTrue(x4text.StartsWith("<?xpacket begin="));
                                Assert.IsTrue(x4text.EndsWith("<?xpacket end='w'?>"));
                                // Console.WriteLine(x4text);
                                break;
                            case "eaf42b5e1c984b88b9fbb7dc406e4d16": // preview data, jpeg 1620x1080
                                var x3 = binaryReader.ReadInt64();
                                Parse(binaryReader, s1 + b0 + 24, length - b0 - 24, depth + " ");
                                break;
                            default:
                                Console.WriteLine("{0}** Unknown UUID {1}, {2} bytes", depth, uuid1, length - b0 - 16);
                                var x1 = binaryReader.ReadBytes((int) (length - b0 - 16));
                                break;
                        }

                        break;

                    case "CTI1":
                        Console.WriteLine("{0}CN: {1}, size 0x{2:X08}", depth, chunkName, length);
                        var data5 = binaryReader.ReadBytes(4);
                        // Assert.AreEqual(s1 + b0 + 4, binaryReader.BaseStream.Position);
                        Parse(binaryReader, s1 + b0 + 4, length - b0 - 4, depth + " ");
                        break;

                    case "CTMD": // Canon Timed MetaData
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y19 = binaryReader.ReadBytes((int) (length - b0));
                        break;

                    case "CMP1":
                        Console.WriteLine("{0}{1}, {2} bytes", depth, chunkName, length - b0);
                        var y20 = binaryReader.ReadBytes((int) (length - b0));
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
    }
}