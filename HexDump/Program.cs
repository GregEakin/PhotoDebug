namespace HexDump
{
    using System;
    using System.IO;

    internal class Program
    {
        public static void TestMethod5(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);

                var address = 0x0L;
                var length = fileStream.Length;
                const int Width = 16;

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                for (var i = 0; i < length; i++)
                {
                    var data = binaryReader.ReadByte();
                    if (data != 0xFE)
                    {
                        continue;
                    }
                    var next = binaryReader.ReadByte();
                    if (next != 0xD4)
                    {
                        continue;
                    }
                    address = binaryReader.BaseStream.Position - 2;
                    break;
                }

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var total = (int)Math.Min(1024, length);
                for (var i = 0; i < total; i += Width)
                {
                    Console.Write("0x{0}: ", (address + i).ToString("X8"));
                    var nextStep = (int)Math.Min(Width, length - i);
                    var data = binaryReader.ReadBytes(nextStep);
                    foreach (var b in data)
                    {
                        Console.Write("{0} ", b.ToString("X2"));
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("...");

                //binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                //var table = new HuffmanTable(binaryReader);
                //Console.WriteLine("Tables {0}", table.Tables.Count());

                //Console.WriteLine("...");

                address = length - 64;
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                for (var i = 0; i < 64; i += Width)
                {
                    Console.Write("0x{0}: ", (address + i).ToString("X8"));
                    var nextStep = (int)Math.Min(Width, length - i);
                    var data = binaryReader.ReadBytes(nextStep);
                    foreach (var b in data)
                    {
                        Console.Write("{0} ", b.ToString("X2"));
                    }
                    Console.WriteLine();
                }
            }
        }

        #region Methods

        private static void Main(string[] args)
        {
            const string Directory = @"..\..\..\Samples\";
            // const string FileName = Directory + "IMG_0503.CR2";
            // const string FileName = Directory + "IMG_0503.JPG";
            // const string FileName = Directory + "IMAG0086.jpg";
            // const string FileName = Directory + "IMG_0511.CR2";
            const string FileName = Directory + "huff_simple0.jpg";

            TestMethod5(FileName);
        }

        #endregion
    }
}