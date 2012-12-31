namespace PhotoLib.Jpeg
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using PhotoLib.Utilities;

    public class StartOfImage : JpegTag
    {
        #region Fields

        private readonly DefineHuffmanTable huffmanTable;

        private readonly ImageData imageData;

        private readonly JfifMarker jfifMarker;

        private readonly StartOfFrame lossless;

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
                    {
                        var nextTag = binaryReader.ReadByte();
                        binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                        Console.WriteLine("NextMark {0}", nextTag.ToString("X2"));
                        switch (nextTag)
                        {
                            case 0xC0:
                            case 0xC3:
                                this.lossless = new StartOfFrame(binaryReader);
                                break;

                            case 0xC4:
                                this.huffmanTable = new DefineHuffmanTable(binaryReader);
                                break;

                            case 0xD9:
                                var x3 = binaryReader.ReadByte();
                                var x4 = binaryReader.ReadByte();
                                break;

                            case 0xDA:
                                this.startOfScan = new StartOfScan(binaryReader);
                                this.imageData = new ImageData(binaryReader, (uint)rawSize);
                                // this.imageData = binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - 2));
                                DoIt();
                                break;

                            case 0xDB:
                                var defineQuantizatonTable = new DefineQuantizationTable(binaryReader);
                                break;

                            case 0xE0:
                                this.jfifMarker = new JfifMarker(binaryReader);
                                break;

                            case 0xE1:
                            case 0xE4:
                            case 0xEC:
                            case 0xEE:
                                var x1 = binaryReader.ReadByte();
                                var x2 = binaryReader.ReadByte();
                                var length1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                                var data = binaryReader.ReadBytes(length1 - 2);
                                break;

                            default:
                                throw new NotImplementedException("Tag 0xFF 0x{0} is not implemented".FormatWith(nextTag.ToString("X2")));
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException("Tag 0x{0} is not implemented".FormatWith(nextMark.ToString("X2")));
                }
            }
        }

        private void DoIt()
        {
            var dicts = new Dictionary<byte, Dictionary<int, DefineHuffmanTable.HCode>>();
            foreach (var table in huffmanTable.Tables)
            {
                var dictionary = DefineHuffmanTable.BuildTree2(table);
                dicts.Add(table.Index, dictionary);
            }

            for (var i = 0; i < (lossless.SamplesPerLine + 7)/8; i++)
            {
                if (dicts.ContainsKey(0x00))
                {
                    // Luminance (Y) - DC
                    this.ReadComponent(dicts[0x00], 1);

                    if (dicts.ContainsKey(0x10))
                    {
                        // Luminance (Y) - AC
                        this.ReadComponent(dicts[0x10], 63);
                    }
                }

                if (dicts.ContainsKey(0x01))
                {
                    // Chrominance (Cb) - DC
                    this.ReadComponent(dicts[0x01], 1);

                    if (dicts.ContainsKey(0x11))
                    {
                        // Chrominance (Cb) - AC
                        this.ReadComponent(dicts[0x11], 63);
                    }
                
                    // Chrominance (Cr) - DC
                    this.ReadComponent(dicts[0x01], 1);

                    if (dicts.ContainsKey(0x11))
                    {
                        // Chrominance (Cr) - AC
                        this.ReadComponent(dicts[0x11], 63);
                    }
                }
            }
        }

        private void ReadComponent(Dictionary<int, DefineHuffmanTable.HCode> dict, int elements)
        {
            var bits = (ushort)0;
            var len = 0;
            var count = 0;

            while (true)
            {
                bits = this.imageData.GetNextBit(bits);
                len++;
                DefineHuffmanTable.HCode hCode;
                if (dict.TryGetValue(bits, out hCode) && hCode.Length == len)
                {
                    if (hCode.Code == 0x00)
                    {
                        // Console.WriteLine("Found {0} {1} EOB", hCode.Code.ToString("X2"), bits.ToString("X4"));
                        break;
                    }

                    var z = this.imageData.GetNextBits(hCode.Code);
                    var value = DefineHuffmanTable.DcValueEncoding(hCode.Code, z);

                    // Console.WriteLine("Found {0} {1} {2}", hCode.Code.ToString("X2"), z.ToString("X4"), value);
                    bits = 0;
                    len = 0;

                    if (++count >= elements)
                    {
                        break;
                    }
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

        public ImageData ImageData
        {
            get
            {
                return imageData;
            }
        }

        public StartOfFrame Lossless
        {
            get
            {
                return lossless;
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
    }
}