﻿// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Ifid1Tests.cs
// AUTHOR:		Greg Eakin

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Ifid1Tests
    {
        [TestMethod]
        public void DumpImage0Test()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            DumpImage1(fileName);
        }

        private static void DumpImage1(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var image = rawImage.Directories.Skip(1).First();
                Assert.AreEqual(2, image.Entries.Length);
                CollectionAssert.AreEqual(
                    new ushort[] { 0x0201, 0x0202 },
                    image.Entries.Select(e => e.TagId).ToArray());

                var offset = image.Entries.Single(e => e.TagId == 0x0201 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(80324u, offset);

                var length = image.Entries.Single(e => e.TagId == 0x0202 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(10334u, length);       

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                DumpImage("Data2.jpg", binaryReader, length);
            }
        }

        private static void DumpImage(string output, BinaryReader binaryReader, uint length)
        {
            using (var x = File.Create(output))
            {
                var bytes = (int)length;
                var buffer = new byte[32768];
                int read;
                while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
                {
                    x.Write(buffer, 0, read);
                    bytes -= read;
                }
            }
        }
    }
}
