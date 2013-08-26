namespace PhotoLib.Tiff
{
    using System;
    using System.IO;
    using System.Linq;

    using PhotoLib.Utilities;

    public class ImageFileDirectory
    {
        #region Fields

        private readonly ImageFileEntry[] entries;

        private readonly uint nextEntry;

        #endregion

        #region Constructors and Destructors

        public ImageFileDirectory(ushort length)
        {
            this.entries = new ImageFileEntry[length];
        }

        public ImageFileDirectory(BinaryReader binaryReader)
        {
            var length = binaryReader.ReadUInt16();
            this.entries = new ImageFileEntry[length];
            for (var i = 0; i < length; i++)
            {
                this.entries[i] = new ImageFileEntry(binaryReader);
            }
            var next = binaryReader.ReadUInt32();
            this.nextEntry = next;

            Console.WriteLine("### Directory {0}, [0x{1}]", length, next.ToString("X8"));
        }

        #endregion

        #region Public Properties

        public ImageFileEntry[] Entries
        {
            get
            {
                return this.entries;
            }
        }

        public uint NextEntry
        {
            get
            {
                return this.nextEntry;
            }
        }

        public ImageFileEntry this[ushort key]
        {
            get
            {
                return this.entries.FirstOrDefault(imageFileEntry => imageFileEntry.TagId == key);
            }
        }

        #endregion

        #region Public Methods and Operators

        public void DumpDirectory(BinaryReader binaryReader)
        {
            const string BlockHeader = "{0,2})  0x{1} {2}: ";
            const string ReferencedItem = "[0x{0}] ({1}): ";
            const string RationalItem = "{0}/{1} = {2}";

            var count = -1;
            foreach (var entry in this.Entries)
            {
                count++;

                if (entry.TagType == 0x04 && entry.TagId == 0x8769) // TIF_EXIF IFD - A pointer to the Exif IFD.
                {
                    Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Image File Directory");
                    Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader);
                }
                if (entry.TagType == 0x07 && entry.TagId == 0x927c) // Makernote.
                {
                    Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Maker note");
                    Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader);
                }
                else
                {
                    switch (entry.TagType)
                    {
                        case 0x01:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Byte 8-bit");
                            Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            break;

                        case 0x02:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Ascii 8-bit");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);

                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            for (var j = 0; j < entry.NumberOfValue - 1; j++)
                            {
                                var us = binaryReader.ReadByte();
                                Console.Write("{0}", (char)us);
                            }
                            binaryReader.ReadByte();
                            Console.WriteLine();
                            break;

                        case 0x03:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "UShort 16-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var us = binaryReader.ReadUInt16();
                                    Console.Write("{0}, ", us);
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x04:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "ULong 32-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var long1 = binaryReader.ReadUInt32();
                                    Console.Write("{0} ", long1.ToString("X4"));
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x05:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Rational 2x32-bit");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), 2);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var us1 = binaryReader.ReadUInt32();
                            var us2 = binaryReader.ReadUInt32();
                            Console.WriteLine(RationalItem, us1, us2, us1 / (double)us2);
                            break;

                        case 0x06:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SByte 8-bit");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x07:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Undefinded");
                            if (entry.NumberOfValue <= 4)
                            {
                                Console.Write("{0}, ", entry.ValuePointer >> 0 & 0xFF);
                                Console.Write("{0}, ", entry.ValuePointer >> 8 & 0xFF);
                                Console.Write("{0}, ", entry.ValuePointer >> 16 & 0xFF);
                                Console.Write("{0}", entry.ValuePointer >> 24 & 0xFF);
                            }
                            else
                            {
                                Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            }
                            Console.WriteLine();
                            break;

                        case 0x08:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SShort 16-bit");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x09:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SLong 32-bit");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x0A:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SRational 2x32-bit");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var s1 = binaryReader.ReadInt32();
                            var s2 = binaryReader.ReadInt32();
                            Console.WriteLine(RationalItem, s1, s2, s1 / (double)s2);
                            break;

                        case 0x0B:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Float 4-Byte");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x0C:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Double 8-Byte");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        default:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Undefined");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));
                    }
                }
            }
        }

        #endregion
    }
}