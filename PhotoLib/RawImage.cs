namespace PhotoLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class RawImage
    {
        #region Fields

        private readonly Dictionary<uint, ImageFileDirectory> directoryList = new Dictionary<uint, ImageFileDirectory>();

        private readonly CR2Header header;

        #endregion

        #region Constructors and Destructors

        public RawImage(BinaryReader binaryReader)
        {
            this.header = new CR2Header(binaryReader);
            var next = this.header.TiffOffset;
            while (next > 0)
            {
                Console.WriteLine("== Tiff Direcotry {2}: 0x{0:x} == 0x{1:x}", next, binaryReader.BaseStream.Position, this.directoryList.Count);
                var dir = new ImageFileDirectory(binaryReader, next);
                this.directoryList.Add(next, dir);
                next = dir.NextEntry;
                Console.Write("===");
            }

            next = header.RawIfdOffset;
            while (next > 0 && !this.directoryList.ContainsKey(next))
            {
                Console.WriteLine("== Raw Direcotry {2}: 0x{0:x} == 0x{1:x}", next, binaryReader.BaseStream.Position, this.directoryList.Count);
                var dir = new ImageFileDirectory(binaryReader, next);
                directoryList.Add(next, dir);
                next = dir.NextEntry;
                Console.Write("===");
            }
        }

        #endregion

        #region Public Properties

        public ICollection<ImageFileDirectory> Directories
        {
            get
            {
                return this.directoryList.Values;
            }
        }

        public CR2Header Header
        {
            get
            {
                return this.header;
            }
        }

        #endregion
    }
}