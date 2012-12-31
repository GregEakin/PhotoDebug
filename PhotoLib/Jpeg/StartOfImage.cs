namespace PhotoLib.Jpeg
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using PhotoLib.Utilities;

    public class StartOfImage : JpegTag
    {
        #region Fields

        private readonly DefineHuffmanTable huffmanTable;

        private readonly IImageData imageData;

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
                        Console.WriteLine("NextMark {0}: 0x{1}", nextTag.ToString("X2"), binaryReader.BaseStream.Position.ToString("X8"));
                        switch (nextTag)
                        {
                            case 0xC0:  // SOF0, Start of Frame 0, Baseline DCT
                            case 0xC3:  // SOF3, Start of Frame 3, Lossless (sequential)
                                this.lossless = new StartOfFrame(binaryReader);
                                break;

                            case 0xC4:  // DHT, Define Huffman Table
                                this.huffmanTable = new DefineHuffmanTable(binaryReader);
                                break;

                            case 0xD9:  // EOI, End of Image
                                var x3 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                                break;

                            case 0xDA:  // SOS, Start of Scan
                                this.startOfScan = new StartOfScan(binaryReader);
                                this.imageData = new LinearImageData(binaryReader, (uint)rawSize);
                                // this.DecodeHuffmanData();
                                // break;
                                return;

                            case 0xDB:  // DQT, Define Quantization Table
                                var defineQuantizatonTable = new DefineQuantizationTable(binaryReader);
                                break;

                            case 0xE0:  // APP0, Application Segment 0, JFIF - JFIF JPEG image, AVI1 - Motion JPEG (MJPG)
                                this.jfifMarker = new JfifMarker(binaryReader);
                                break;

                            case 0xE1:  // APP1, Application Segment 1, EXIF Metadata, TIFF IFD format,JPEG Thumbnail (160x120), Adobe XMP
                            case 0xE4:  // APP4, Application Segment 4, (Not common)
                            case 0xEC:  // APP12, Application Segment 12, Picture Info (older digicams), Photoshop Save for Web: Ducky
                            case 0xEE:  // APP14, Application Segment 14, (Not common)
                                var x1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
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

        #endregion

        #region Public Properties

        public DefineHuffmanTable HuffmanTable
        {
            get
            {
                return huffmanTable;
            }
        }

        public IImageData ImageData
        {
            get
            {
                return this.imageData;
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

        #region Methods

        private void DecodeHuffmanData()
        {
            for (var i = 0; i < (this.lossless.SamplesPerLine + 7) / 8; i++)
            {
                if (this.huffmanTable.Tables.ContainsKey(0x00))
                {
                    // Luminance (Y) - DC
                    this.ReadComponent(this.huffmanTable.Tables[0x00].Dictionary, 1);

                    if (this.huffmanTable.Tables.ContainsKey(0x10))
                    {
                        // Luminance (Y) - AC
                        this.ReadComponent(this.huffmanTable.Tables[0x10].Dictionary, 63);
                    }
                }

                if (this.huffmanTable.Tables.ContainsKey(0x01))
                {
                    // Chrominance (Cb) - DC
                    this.ReadComponent(this.huffmanTable.Tables[0x01].Dictionary, 1);

                    if (this.huffmanTable.Tables.ContainsKey(0x11))
                    {
                        // Chrominance (Cb) - AC
                        this.ReadComponent(this.huffmanTable.Tables[0x11].Dictionary, 63);
                    }

                    // Chrominance (Cr) - DC
                    this.ReadComponent(this.huffmanTable.Tables[0x01].Dictionary, 1);

                    if (this.huffmanTable.Tables.ContainsKey(0x11))
                    {
                        // Chrominance (Cr) - AC
                        this.ReadComponent(this.huffmanTable.Tables[0x11].Dictionary, 63);
                    }
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
                bits = this.imageData.GetNextBit(bits);
                len++;
                HuffmanTable.HCode hCode;
                if (dict.TryGetValue(bits, out hCode) && hCode.Length == len)
                {
                    if (hCode.Code == 0x00)
                    {
                        // Console.WriteLine("Found {0} {1} EOB", hCode.Code.ToString("X2"), bits.ToString("X4"));
                        break;
                    }

                    var z = this.imageData.GetSetOfBits(hCode.Code);
                    var value = Jpeg.HuffmanTable.DcValueEncoding(hCode.Code, z);

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
    }
}