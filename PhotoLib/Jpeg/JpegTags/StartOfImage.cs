// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		StartOfImage.cs
// AUTHOR:		Greg Eakin

namespace PhotoLib.Jpeg.JpegTags
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// SOI 0xFFD8
    /// </summary>
    public class StartOfImage : JpegTag
    {
        private readonly JfifMarker _jfifMarker;

        public StartOfImage(BinaryReader binaryReader, uint address, uint length)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xD8)
                throw new ArgumentException();

            var start = binaryReader.BaseStream.Position;
            while (binaryReader.BaseStream.Position < start + length - 2)
            {
                var pos = binaryReader.BaseStream.Position;
                var nextMark = binaryReader.ReadByte();
                if (nextMark != 0xFF)
                    throw new NotImplementedException($"Tag 0x{nextMark:X2} is not implemented");

                var nextTag = binaryReader.ReadByte();
                binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                Console.WriteLine($"NextMark {nextTag:X2}: 0x{binaryReader.BaseStream.Position:X8}");
                switch (nextTag)
                {
                    case 0xC0: // SOF0, Start of Frame 0, Baseline DCT
                    case 0xC3: // SOF3, Start of Frame 3, Lossless (sequential)
                        StartOfFrame = new StartOfFrame(binaryReader);
                        var image = StartOfFrame.SamplesPerLine * StartOfFrame.ScanLines;
                        Console.WriteLine("Image = {0} * {1} = {2}", StartOfFrame.ScanLines,
                            StartOfFrame.SamplesPerLine, image);
                        break;

                    case 0xC4: // DHT, Define Huffman Table
                        HuffmanTable = new DefineHuffmanTable(binaryReader);
                        break;

                    case 0xD9: // EOI, End of Image
                        var x3 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                        break;

                    case 0xDA: // SOS, Start of Scan
                        StartOfScan = new StartOfScan(binaryReader);
                        var rawSize = address + length - binaryReader.BaseStream.Position;
                        ImageData = new ImageData(binaryReader, (uint)rawSize);
                        DecodeHuffmanData();
                        break;

                    case 0xDB: // DQT, Define Quantization Table
                        QuantizationTable = new DefineQuantizationTable(binaryReader);
                        break;

                    case 0xE0: // APP0, Application Segment 0, JFIF - JFIF JPEG image, AVI1 - Motion JPEG (MJPG)
                        _jfifMarker = new JfifMarker(binaryReader);
                        break;


                    case 0xE1: // APP1, Application Segment 1, EXIF Metadata, TIFF IFD format,JPEG Thumbnail (160x120), Adobe XMP
                    case 0xE2: // APP2, Application Segment 2,
                    case 0xE4: // APP4, Application Segment 4, (Not common)
                    case 0xEC: // APP12, Application Segment 12, Picture Info (older digicams), Photoshop Save for Web: Ducky
                    case 0xEE: // APP14, Application Segment 14, (Not common)
                        var x1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                        var length1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                        var data = binaryReader.ReadBytes(length1 - 2);
                        break;

                    case 0xFE:
                        var comment = new Comment(binaryReader);
                        break;

                    default:
                        throw new NotImplementedException($"Tag 0xFF 0x{nextTag:X2} is not implemented");
                }
            }
        }

        public DefineHuffmanTable HuffmanTable { get; }

        public DefineQuantizationTable QuantizationTable { get; }

        public ImageData ImageData { get; set; }

        public StartOfFrame StartOfFrame { get; }

        public StartOfScan StartOfScan { get; }

        public void DecodeHuffmanData()
        {
            Console.WriteLine("cr2_slice: ");

            Console.WriteLine("Frame Components: ");
            for (var i = 0; i < StartOfFrame.Components.Length; i++)
            {
                var component = StartOfFrame.Components[i];
                Console.WriteLine("  {0}: id {1}, HF {2}, VF {3}", i, component.TableId, component.HFactor, component.VFactor);
            }
            var tables = StartOfFrame.Components.Select(component => component.TableId).Distinct().Count();
            Console.WriteLine("Tables: rows {0}, unique entries {1}", StartOfFrame.Components.Length, tables);

            Console.WriteLine("Huffman Tables");
            foreach (var table in HuffmanTable.Tables)
            {
                var type = (table.Value.Index >> 4) == 0 ? "DC" : "AC";
                var id = (table.Value.Index & 0x0F) == 0 ? "Y Component" : "Color Components";

                Console.WriteLine("    0x{0:X2}, {1}, {2}", table.Key, type, id);
            }

            if (tables * 2 != HuffmanTable.Tables.Count)
            {
                throw new ArgumentException();
            }

            //Console.WriteLine("Scan Components: ");
            //for (var i = 0; i < startOfScan.Components.Length; i++)
            //{
            //    var component = startOfScan.Components[i];
            //    Console.WriteLine("  {0}: id {1}, DC {2}, AC {3}", i, component.Id, component.Dc, component.Ac);
            //}
            //Console.WriteLine("  {0}, {1}, {2}", startOfScan.Bb1, startOfScan.Bb2, startOfScan.Bb3);

            switch (tables)
            {
                case 1:
                    {
                        //      V-- type (0=DC, 1=AC)
                        //   1, 0, 1, 1
                        //   2, 0, 1, 1

                        //   1, 0, 2, 1
                        //   2, 0, 1, 1
                        //   3, 0, 1, 1

                        //   1, 0, 1, 1
                        //   2, 0, 1, 1
                        //   3, 0, 1, 1
                        //   4, 0, 1, 1

                        HuffmanTable luminanceDc;
                        var table1 = HuffmanTable.Tables.TryGetValue(0x00, out luminanceDc);
                        HuffmanTable chrominanceDc;
                        var table3 = HuffmanTable.Tables.TryGetValue(0x01, out chrominanceDc);

                        if (!table1 || !table3)
                            throw new Exception("Didn't read the table.");

                        TableTwo(StartOfFrame.Components, luminanceDc, chrominanceDc);
                    }
                    break;

                case 2:
                    {
                        //      V-- type (0=DC, 1=AC)
                        //   1, 0, 1, 1
                        //   2, 1, 1, 1
                        //   3, 1, 1, 1

                        HuffmanTable luminanceDc;
                        var table1 = HuffmanTable.Tables.TryGetValue(0x00, out luminanceDc);
                        HuffmanTable chrominanceDc;
                        var table3 = HuffmanTable.Tables.TryGetValue(0x01, out chrominanceDc);
                        HuffmanTable luminanceAc;
                        var table2 = HuffmanTable.Tables.TryGetValue(0x10, out luminanceAc);
                        HuffmanTable chrominanceAc;
                        var table4 = HuffmanTable.Tables.TryGetValue(0x11, out chrominanceAc);

                        if (!table1 || !table2 || !table3 || !table4)
                            throw new Exception("Didn't read the table.");

                        TableFour(StartOfFrame.Components, luminanceDc, luminanceAc, chrominanceDc, chrominanceAc);
                    }
                    break;

                default:
                    Console.WriteLine("Frame Components: ");
                    for (var i = 0; i < StartOfFrame.Components.Length; i++)
                    {
                        var component = StartOfFrame.Components[i];
                        Console.WriteLine(" {0}, {1}, {2}, {3}", i, component.TableId, component.HFactor, component.VFactor);
                    }
                    Console.WriteLine("Scan Components: ");
                    for (var i = 0; i < StartOfScan.Components.Length; i++)
                    {
                        var component = StartOfScan.Components[i];
                        Console.WriteLine(" {0}, {1}, {2}, {3}", i, component.Id, component.Dc, component.Ac);
                    }
                    Console.WriteLine("Tables: {0}", HuffmanTable.Tables.Count);

                    throw new NotImplementedException($"Subsampling not implemented {StartOfFrame.Components.Length}");
            }
        }

        private void TableTwo(StartOfFrame.Component[] components, HuffmanTable luminanceDc, HuffmanTable luminanceAc)
        {
            var i = 0;
            while (!ImageData.EndOfFile)
            {
                try
                {
                    // Luminance (Y) - DC
                    var dc = ReadDcComponent(luminanceDc.Dictionary);

                    // Luminance (Y) - AC
                    var ac = ReadAcComponent(luminanceAc.Dictionary);

                    i++;
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Crash {0}", exception);
                    throw;
                }
            }

            Console.WriteLine("Count = {0}", i);
        }

        private void TableFour(StartOfFrame.Component[] components, HuffmanTable luminanceDc, HuffmanTable luminanceAc, HuffmanTable chrominanceDc, HuffmanTable chrominanceAc)
        {
            var lastYdc = 0;

            // read 8x8 blocks
            var width = StartOfFrame.SamplesPerLine / 8;
            var length = StartOfFrame.ScanLines / 8;
            var size = width * length;
            for (var i = 0; i < size; i+=12)
            //while (!ImageData.EndOfFile)
            {
                Console.WriteLine("Reading {0} {1}", ImageData.Index, i);
                try
                {
                    // Luminance (Y) - DC
                    var ydc = lastYdc + ReadDcComponent(luminanceDc.Dictionary);
                    lastYdc = ydc;

                    // Luminance (Y) - AC
                    var yac = ReadAcComponent(luminanceAc.Dictionary);

                    // Chrominance (Cb) - DC
                    var cbdc = ReadDcComponent(chrominanceDc.Dictionary);

                    // Chrominance (Cb) - AC
                    var cbac = ReadAcComponent(chrominanceAc.Dictionary);

                    // Chrominance (Cr) - DC
                    var crdc = ReadDcComponent(chrominanceDc.Dictionary);

                    // Chrominance (Cr) - AC
                    var crac = ReadAcComponent(chrominanceAc.Dictionary);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Crash {0}", exception);
                    // Console.WriteLine("  i={0}, size={1}", i, size);

                    throw;
                }
            }
        }

        private int ReadDcComponent(IReadOnlyDictionary<int, HuffmanTable.HCode> dict)
        {
            var value = 0;

            var bits = (ushort)0;
            var len = 0;

            while (!ImageData.EndOfFile)
            {
                bits = ImageData.GetNextShort(bits);
                len++;

                HuffmanTable.HCode hCode;
                if (!dict.TryGetValue(bits, out hCode) || hCode.Length != len)
                {
                    continue;
                }

                if (hCode.Code == 0x00)
                {
                    break;
                }

                var z = ImageData.GetSetOfBits(hCode.Code);
                value = Jpeg.HuffmanTable.DcValueEncoding(hCode.Code, z);
                break;
            }

            return value;
        }

        private int[] ReadAcComponent(IReadOnlyDictionary<int, HuffmanTable.HCode> dict)
        {
            var value = new int[63];

            var bits = (ushort)0;
            var len = 0;
            var count = 0;

            while (true)
            {
                bits = ImageData.GetNextShort(bits);
                len++;

                if (len > 16)
                    throw new Exception($"Didn't find the code! len: {len}, bits: 0x{bits:X8}");

                HuffmanTable.HCode hCode;
                if (!dict.TryGetValue(bits, out hCode) || hCode.Length != len)
                    continue;

                if (hCode.Code == 0x00)
                    break;

                var z = ImageData.GetSetOfBits(hCode.Code);
                value[count] = Jpeg.HuffmanTable.DcValueEncoding(hCode.Code, z);

                if (++count >= 63)
                    break;

                len = 0;
                bits = 0;
            }

            return value;
        }

        public short ProcessColor(byte index)
        {
            var table = HuffmanTable.Tables[index];
            var hufBits = ImageData.GetValue(table);
            var difCode = ImageData.GetValue(hufBits);
            var difValue = Jpeg.HuffmanTable.DecodeDifBits(hufBits, difCode);
            return difValue;
        }
    }
}