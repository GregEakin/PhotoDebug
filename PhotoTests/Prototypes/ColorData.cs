// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		ColorData.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class ColorData
    {
        [TestMethod]
        public void DumpColorData()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();
                image.DumpDirectory(binaryReader);

                // modelId, modelName, jpeg_bits, jpeg_wide, jpeg_high, jpeg_comp, slices[0], slices[1], slices[2], jpeg_hsf, jpeg_vsf, sensorLeftBorder, sensorTopBorder, sensorBottomBorder, sensorRightBorder,blackMaskLeftBorder,blackMasktopBorder,blackMaskBottomBorder,blackMaskRightBorder, imageWidth, imageHeight, vShift
                // 0x80000250r, Canon EOS 7D, 2047 2047 2047 2047, 15400, 0.6844 - 0.0996 - 0.0856 - 0.3876 1.1761 0.2396 - 0.0593 0.1772 0.6198
                // 0x80000285r, Canon EOS 5D Mark III, 2047 2047 2047 2048, 15000, 0.6722 -0.0635 -0.0963 -0.4287 1.246 0.2028 -0.0908 0.2162 0.5668

                //var gps = image.Entries.Single(e => e.TagId == 0x8825 && e.TagType == 4).ValuePointer;
                //// Assert.AreEqual(70028u, gps);

                //binaryReader.BaseStream.Seek(gps, SeekOrigin.Begin);

                //var tags = new ImageFileDirectory(binaryReader);
                //tags.DumpDirectory(binaryReader);

                //Assert.AreEqual(1, tags.Entries.Length);
                //CollectionAssert.AreEqual(
                //    new ushort[] { 0x0000 }, tags.Entries.Select(e => e.TagId).ToArray());

                //var imageFileEntry = tags.Entries.Single(e => e.TagId == 0x0000 && e.TagType == 1);
                //Assert.AreEqual(4u, imageFileEntry.NumberOfValue);
                //// Assert.AreEqual(0x302u, imageFileEntry.ValuePointer);
                //var xmpData = RawImage.ReadBytes(binaryReader, imageFileEntry);
                //CollectionAssert.AreEqual(new byte[] { 0x01, 0x00, 0x0F, 0xA2 }, xmpData);
            }
        }
    }
}
