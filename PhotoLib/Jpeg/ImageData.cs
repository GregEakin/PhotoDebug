namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class ImageData
    {
        #region Fields

        private readonly byte[] rawData;

        #endregion

        #region Constructors and Destructors

        public ImageData(BinaryReader binaryReader, uint rawSize)
        {
            rawData = binaryReader.ReadBytes((int)rawSize);

            // GetBits(rawData);
            // GetLosslessJpgRow(null, rawData, TB0, TL0, TB1, TL1, Prop);

            // for (var iRow = 0; iRow < height; iRow++)
            {
                // var rowBuf = new ushort[width];
                // GetLosslessJpgRow(rowBuf, rawData, TL0, TB0, TL1, TB1, Prop);
                // PutUnscrambleRowSlice(rowBuf, imageData, iRow, Prop);
            }

            for (var i = 0; i < Math.Min(20, rawSize); i++)
            {
                var value = rawData[i];
                if (value == 0xFF)
                {
                    var code = rawData[++i];
                    if (code != 0)
                    {
                        value = rawData[++i];
                    }
                }
                Console.Write("{0} ", value.ToString("X2"));
            }
            Console.WriteLine();
        }

        #endregion

        #region Public Properties

        public byte[] RawData
        {
            get
            {
                return rawData;
            }
        }

        #endregion
    }
}