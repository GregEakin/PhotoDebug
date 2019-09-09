// Log File Viewer - MemoryTests.cs
// 
// Copyright ©  Greg Eakin.
// 
// Greg Eakin <greg@gdbtech.info>
// 
// All Rights Reserved.

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhotoTests.CanonR
{
    [TestClass]
    public class MemoryTests
    {
        private const string FileNameRaw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0001.CR3";
        private const string FileNameSraw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0002.CR3";
        private const string FileNameDualRaw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0003.CR3";
        private const string FileNameDualSraw = @"D:\Users\Greg\Pictures\2019-09-02\IMG_0004.CR3";
        private const string FileName = FileNameRaw;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
                throw new ArgumentException("{0} doesn't exists!", FileName);

            Console.WriteLine("FileName = {0}", Path.GetFileName(FileName));
            Console.WriteLine("Directory = {0}", Path.GetDirectoryName(FileName));
            Console.WriteLine("FileModifyDate = {0}", File.GetLastWriteTime(FileName));
            Console.WriteLine("FileAccessDate = {0}", File.GetLastAccessTime(FileName));
            Console.WriteLine("FileCreateDate = {0}", File.GetCreationTime(FileName));
        }

        [TestMethod]
        public void ReadSteam()
        {
            // var memory = new Memory<byte>(new byte[50]);
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                // fileStream.ReadAsync()
            }

        }

        [TestMethod]
        public void MemoryMappedFilesAccessorTest()
        {
            // throws ArgumentException, IOException, PathTooLongException, SecurityException
            // Share across process name "ImgA"
            using (var mmf = MemoryMappedFile.CreateFromFile(FileName, FileMode.Open, "ImgA"))
            using (var accessor = mmf.CreateViewAccessor(0, 0,  MemoryMappedFileAccess.Read))
            {
                // struct Block {
                //   int length;
                //   char[4] type;
                //   char[4] majorBrand;
                //   int version;
                //   char[4][] brands;  // Number of items = (length - 16) / 4
                // }

                //int blockSize = Marshal.SizeOf(typeof(Block));
                //Block block;
                //for (long i = 0; i < length; i += blockSize)
                //{
                //    accessor.Read(i, out block);
                //    block.Brighten(10);
                //    accessor.Write(i, ref block);
                //}

                Console.WriteLine("Is little endian {0}", BitConverter.IsLittleEndian);
                accessor.Read(0, out int length);      // This reads little endian data, the file is big endian
                Console.WriteLine("length = 0x{0:x8}", length);
            }
        }

        [TestMethod]
        public void MemoryMappedFilesStreamTest()
        {
            // throws ArgumentException, IOException, PathTooLongException, SecurityException
            // Share across process name "ImgA"
            using (var mmf = MemoryMappedFile.CreateFromFile(FileName, FileMode.Open, "ImgA"))
            using (var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
            using (var binaryReader = new BigEndianBinaryReader(stream))
            {
                Console.WriteLine("FileSize {0}", stream.Length);

                {
                    var fileTypeBox = new FileTypeBox(binaryReader);
                    Console.WriteLine("Tag: {0}, {1} bytes", fileTypeBox.Type, fileTypeBox.Length);
                    Assert.AreEqual("ftyp", fileTypeBox.Type);

                    Assert.AreEqual("crx ", fileTypeBox.MajorBrand);
                    Assert.AreEqual(1, fileTypeBox.Version);

                    CollectionAssert.AreEquivalent(new[] { "crx ", "isom" }, fileTypeBox.CompatibleBrands);
                }

            }
        }
    }
}