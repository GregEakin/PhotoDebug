namespace HexDump
{
    using System;
    using System.IO;
    using System.Linq;

    using PhotoLib.Jpeg;

    internal class Program
    {
        #region Public Methods and Operators

        public static void TestMethod5(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var address = 0L;
                var length = fileStream.Length;
                const int Width = 16;

                var binaryReader = new BinaryReader(fileStream);

                //binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                //for (var i = 0; i < length; i++)
                //{
                //    var data = binaryReader.ReadByte();
                //    if (data != 0xFF)
                //    {
                //        continue;
                //    }
                //    var next = binaryReader.ReadByte();
                //    if (next != 0xC4)
                //    {
                //        continue;
                //    }
                //    address = binaryReader.BaseStream.Position - 2;
                //    break;
                //}

                //binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                //var table = new HuffmanTable(binaryReader);
                //Console.WriteLine("Tables {0}", table.Tables.Count());

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                for (var i = 0; i < 256; i += Width)
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

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            TestMethod5(@"C:\Users\Greg\Pictures\IMG_0503.CR2");
            // TestMethod5(@"C:\Users\Greg\Pictures\IMG_0503.JPG");
            // TestMethod5(@"C:\Users\Greg\Pictures\IMAG0086.jpg");
            // private const string FileName = @"C:\Users\Greg\Downloads\huff_simple0.jpg";
        }

        #endregion
    }
}