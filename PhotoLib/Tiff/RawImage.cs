namespace PhotoLib.Tiff
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
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                this.directoryList.Add(next, dir);
                next = dir.NextEntry;
            }

            next = this.header.RawIfdOffset;
            while (next > 0 && !this.directoryList.ContainsKey(next))
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                this.directoryList.Add(next, dir);
                next = dir.NextEntry;
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

        #region Public Methods and Operators

        public void DumpHeader(BinaryReader binaryReader)
        {
            foreach (var item in directoryList)
            {
                Console.WriteLine("== Tiff Direcotry [0x{0}]:", item.Key.ToString("X8"));
                item.Value.DumpDirectory(binaryReader);
            }
        }

        #endregion
    }
}