namespace PhotoLib.Tiff
{
    using System;
    using System.IO;

    public class RawData
    {
        #region Fields

        private readonly byte[] data;

        #endregion

        #region Constructors and Destructors

        public RawData(BinaryReader binaryReader, int height, int x, int y, int z)
        {
            var width = x * y + z;
            data = new byte[height * width];

            for (var block = 0; block < x; block++)
            {
                for (var row = 0; row < height; row++)
                {
                    var b = binaryReader.ReadBytes(y);
                    var b1 = row * width + block * y;
                    Array.Copy(b, 0L, this.data, b1, y);
                }
            }
            for (var row = 0; row < height; row++)
            {
                var c = binaryReader.ReadBytes(z);
                var c1 = row * width + x * y;
                Array.Copy(c, 0L, this.data, c1, z);
            }
        }

        #endregion

        #region Public Properties

        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        #endregion
    }
}