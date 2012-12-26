namespace PhotoTests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib;

    [TestClass]
    public class JpegFileTests
    {
        #region Constants

        private const string FileName = @"C:\Users\Greg\Downloads\huff_simple0.jpg";

        #endregion

        #region Public Methods and Operators

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
            {
                throw new ArgumentException("{0} doesn't exists!", FileName);
            }
        }

        [TestMethod]
        public void DumpMethod1()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    var address = 0;
                    while (true)
                    {
                        Console.Write("{0,4}h: ", address.ToString("X4"));
                        for (var i = 0; i < 16; i++)
                        {
                            var x = binaryReader.ReadByte();
                            Console.Write("{0,2} ", x.ToString("X2"));
                            address++;
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        #endregion
    }
}