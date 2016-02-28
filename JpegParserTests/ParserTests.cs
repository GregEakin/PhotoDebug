using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JpegParser;
using System.IO.MemoryMappedFiles;

namespace JpegParserTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        [Ignore]
        public void JpegSetup()
        {
            const string Directory = @"..\..\..\Samples\";
            const string FileName2 = Directory + "huff_simple0.jpg";
            // const string FileName2 = Directory + "IMAG0086.jpg";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);

                var parser = new Parser(binaryReader);
                parser.JpegData();
            }
        }

        [TestMethod]
        [Ignore]
        public void MemoryFile()
        {
            const string Directory = @"..\..\..\Samples\";
            const string FileName2 = Directory + "huff_simple0.jpg";

            long offset = 0x10000000; // 256 megabytes
            long length = 0x20000000; // 512 megabytes

            using (var mmf = MemoryMappedFile.CreateFromFile(FileName2, FileMode.Open, "ImgA"))
            {
                // Create a random access view, from the 256th megabyte (the offset)
                // to the 768th megabyte (the offset plus length).
                using (var accessor = mmf.CreateViewAccessor(offset, length))
                {
                    //var colorSize = Marshal.SizeOf(typeof(MyColor));

                    //// Make changes to the view.
                    //for (var i = 0L; i < length; i += colorSize)
                    //{
                    //    MyColor color;
                    //    accessor.Read(i, out color);
                    //    color.Brighten(10);
                    //    accessor.Write(i, ref color);
                    //}
                }
            }
        }
    }
}
