namespace PhotoLib.Jpeg
{
    public interface IImageData
    {
        #region Public Properties

        bool EndOfFile { get; }

        byte[] RawData { get; }

        int Index { get; }

        #endregion

        #region Public Methods and Operators

        bool GetNextBit();

        ushort GetNextBit(ushort lastBit);

        byte GetNextByte();

        ushort GetSetOfBits(ushort total);

        #endregion
    }
}