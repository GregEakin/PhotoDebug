namespace PhotoLib.Jpeg
{
    public interface IImageData
    {
        bool GetNextBit();

        ushort GetNextBit(ushort lastBit);

        byte GetNextByte();

        ushort GetSetOfBits(ushort total);

        bool EndOfFile { get; }
    }
}