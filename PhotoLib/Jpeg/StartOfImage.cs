// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		StartOfImage.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

using PhotoLib.Utilities;

namespace PhotoLib.Jpeg
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// SOI 0xFFD8
    /// </summary>
    public class StartOfImage : JpegTag
    {
        #region Fields

        private readonly DefineHuffmanTable huffmanTable;

        private readonly DefineQuantizationTable quantizationTable;

        private ImageData imageData;

        private readonly JfifMarker jfifMarker;

        private readonly StartOfFrame startOfFrame;

        private readonly StartOfScan startOfScan;

        #endregion

        #region Constructors and Destructors

        public StartOfImage(BinaryReader binaryReader, uint address, uint length)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xD8)
            {
                throw new ArgumentException();
            }

            Console.WriteLine("SOI: 0x{0}", binaryReader.BaseStream.Position.ToString("X8"));

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                var pos = binaryReader.BaseStream.Position;
                var rawSize = address + length - pos;
                var nextMark = binaryReader.ReadByte();
                if (nextMark == 0xFF)
                {
                    var nextTag = binaryReader.ReadByte();
                    binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                    Console.WriteLine("NextMark {0}: 0x{1}", nextTag.ToString("X2"), binaryReader.BaseStream.Position.ToString("X8"));
                    switch (nextTag)
                    {
                        case 0xC0: // SOF0, Start of Frame 0, Baseline DCT
                        case 0xC3: // SOF3, Start of Frame 3, Lossless (sequential)
                            this.startOfFrame = new StartOfFrame(binaryReader);
                            var image = startOfFrame.SamplesPerLine * startOfFrame.ScanLines;
                            Console.WriteLine("Image = {0} * {1} = {2}", startOfFrame.ScanLines, startOfFrame.SamplesPerLine, image);
                            break;

                        case 0xC4: // DHT, Define Huffman Table
                            this.huffmanTable = new DefineHuffmanTable(binaryReader);
                            break;

                        case 0xD9: // EOI, End of Image
                            var x3 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                            break;

                        case 0xDA: // SOS, Start of Scan
                            this.startOfScan = new StartOfScan(binaryReader);
                            this.imageData = new ImageData(binaryReader, (uint)rawSize);
                            this.DecodeHuffmanData();
                            break;

                        case 0xDB: // DQT, Define Quantization Table
                            this.quantizationTable = new DefineQuantizationTable(binaryReader);
                            break;

                        case 0xE0: // APP0, Application Segment 0, JFIF - JFIF JPEG image, AVI1 - Motion JPEG (MJPG)
                            this.jfifMarker = new JfifMarker(binaryReader);
                            break;

                        case 0xE1: // APP1, Application Segment 1, EXIF Metadata, TIFF IFD format,JPEG Thumbnail (160x120), Adobe XMP
                        case 0xE4: // APP4, Application Segment 4, (Not common)
                        case 0xEC: // APP12, Application Segment 12, Picture Info (older digicams), Photoshop Save for Web: Ducky
                        case 0xEE: // APP14, Application Segment 14, (Not common)
                            var x1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                            var length1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                            var data = binaryReader.ReadBytes(length1 - 2);
                            break;

                        default:
                            throw new NotImplementedException("Tag 0xFF 0x{0} is not implemented".FormatWith(nextTag.ToString("X2")));
                    }
                }
                else
                {
                    throw new NotImplementedException("Tag 0x{0} is not implemented".FormatWith(nextMark.ToString("X2")));
                }
            }
        }

        #endregion

        #region Public Properties

        public DefineHuffmanTable HuffmanTable
        {
            get
            {
                return huffmanTable;
            }
        }

        public DefineQuantizationTable QuantizationTable
        {
            get
            {
                return quantizationTable;
            }
        }

        public ImageData ImageData
        {
            get
            {
                return this.imageData;
            }
            set
            {
                this.imageData = value;
            }
        }

        public StartOfFrame StartOfFrame
        {
            get
            {
                return this.startOfFrame;
            }
        }

        public StartOfScan StartOfScan
        {
            get
            {
                return startOfScan;
            }
        }

        #endregion

        #region Methods

        public void DecodeHuffmanData()
        {
            Console.WriteLine("Frame Components: ");
            for (var i = 0; i < startOfFrame.Components.Length; i++)
            {
                var component = startOfFrame.Components[i];
                Console.WriteLine("  {0}: id {1}, HF {2}, VF {3}", i, component.TableId, component.HFactor, component.VFactor);
            }
            var tables = this.startOfFrame.Components.Select(component => component.TableId).Distinct().Count();
            Console.WriteLine("Tables: rows {0}, unique entries {1}", startOfFrame.Components.Length, tables);

            Console.WriteLine("Huffman Tables");
            foreach (var table in huffmanTable.Tables)
            {
                var type = (table.Value.Index >> 4) == 0 ? "DC" : "AC";
                var id = (table.Value.Index & 0x0F) == 0 ? "Y Component" : "Color Components";

                Console.WriteLine("    0x{0}, {1}, {2}", table.Key.ToString("X2"), type, id);
            }

            if (tables * 2 != huffmanTable.Tables.Count)
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
                        var table1 = this.huffmanTable.Tables.TryGetValue(0x00, out luminanceDc);
                        HuffmanTable chrominanceDc;
                        var table3 = this.huffmanTable.Tables.TryGetValue(0x01, out chrominanceDc);

                        this.TableTwo(startOfFrame.Components, luminanceDc, chrominanceDc);
                    }
                    break;

                case 2:
                    {
                        //      V-- type (0=DC, 1=AC)
                        //   1, 0, 1, 1
                        //   2, 1, 1, 1
                        //   3, 1, 1, 1

                        HuffmanTable luminanceDc;
                        var table1 = this.huffmanTable.Tables.TryGetValue(0x00, out luminanceDc);
                        HuffmanTable chrominanceDc;
                        var table3 = this.huffmanTable.Tables.TryGetValue(0x01, out chrominanceDc);
                        HuffmanTable luminanceAc;
                        var table2 = this.huffmanTable.Tables.TryGetValue(0x10, out luminanceAc);
                        HuffmanTable chrominanceAc;
                        var table4 = this.huffmanTable.Tables.TryGetValue(0x11, out chrominanceAc);

                        //  this.TableFour(startOfFrame.Components, luminanceDc, luminanceAc, chrominanceDc, chrominanceAc);
                    }
                    break;

                default:
                    Console.WriteLine("Frame Components: ");
                    for (var i = 0; i < this.startOfFrame.Components.Length; i++)
                    {
                        var component = startOfFrame.Components[i];
                        Console.WriteLine(" {0}, {1}, {2}, {3}", i, component.TableId, component.HFactor, component.VFactor);
                    }
                    Console.WriteLine("Scan Components: ");
                    for (var i = 0; i < startOfScan.Components.Length; i++)
                    {
                        var component = startOfScan.Components[i];
                        Console.WriteLine(" {0}, {1}, {2}, {3}", i, component.Id, component.Dc, component.Ac);
                    }
                    Console.WriteLine("Tables: {0}", huffmanTable.Tables.Count);

                    throw new NotImplementedException("Subsampling not implemented {0}".FormatWith(this.startOfFrame.Components.Length));
            }
        }

        private void TableTwo(StartOfFrame.Component[] components, HuffmanTable luminanceDc, HuffmanTable luminanceAc)
        {
            var width = (this.startOfFrame.SamplesPerLine + 7) / 8;

            for (var j = 0; j < startOfFrame.ScanLines; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    // Luminance (Y) - DC
                    this.ReadComponent(luminanceDc.Dictionary, 1);

                    // Luminance (Y) - AC
                    this.ReadComponent(luminanceAc.Dictionary, 1);
                }
            }
        }

        private void TableFour(StartOfFrame.Component[] components, HuffmanTable luminanceDc, HuffmanTable luminanceAc, HuffmanTable chrominanceDc, HuffmanTable chrominanceAc)
        {
            var width = (this.startOfFrame.SamplesPerLine + 7) / 8;

            for (var j = 0; j < startOfFrame.ScanLines; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    // Luminance (Y) - DC
                    this.ReadComponent(luminanceDc.Dictionary, 1);

                    // Luminance (Y) - AC
                    this.ReadComponent(luminanceAc.Dictionary, 63);

                    // Chrominance (Cb) - DC
                    this.ReadComponent(chrominanceDc.Dictionary, 1);

                    // Chrominance (Cb) - AC
                    this.ReadComponent(chrominanceAc.Dictionary, 63);

                    // Chrominance (Cr) - DC
                    this.ReadComponent(chrominanceDc.Dictionary, 1);

                    // Chrominance (Cr) - AC
                    this.ReadComponent(chrominanceAc.Dictionary, 63);
                }
            }
        }

        private void ReadComponent(IReadOnlyDictionary<int, HuffmanTable.HCode> dict, int elements)
        {
            var bits = (ushort)0;
            var len = 0;
            var count = 0;

            while (true)
            {
                bits = this.imageData.GetNextShort(bits);
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

                var z = this.imageData.GetSetOfBits(hCode.Code);
                var value = Jpeg.HuffmanTable.DcValueEncoding(hCode.Code, z);
                // Console.WriteLine("Found code:{0} bits:{1} value:{2}", hCode.Code.ToString("X2"), z.ToString("X4"), value);

                if (++count >= elements)
                {
                    break;
                }

                len = 0;
            }
        }

        #endregion
    }
}