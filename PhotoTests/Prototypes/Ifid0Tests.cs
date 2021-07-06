// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Ifid0Tests.cs
// AUTHOR:		Greg Eakin

using Xunit;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    
    public class Ifid0Tests
    {
        [Fact]
        public void DumpImage0Test()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            DumpImage0(fileName);
        }

        private static void DumpImage0(string fileName)
        {
            using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // Images #0 and #1 are compressed in lossy (classic) JPEG
            var image = rawImage.Directories.First();

            Assert.Equal(18, image.Entries.Length);
            Assert.Equal(
                new ushort[]
                {
                    0x0100, 0x0101, 0x0102, 0x0103, 0x010F, 0x0110, 0x0111, 0x0112, 0x0117, 0x011A, 0x011B,
                    0x128, 0x0132, 0x013B, 0x02BC, 0x8298, 0x8769, 0x8825
                },
                image.Entries.Select(e => e.TagId).ToArray());

            var imageWidth = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
            Assert.Equal(5760u, imageWidth);

            var imageLength = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
            Assert.Equal(3840u, imageLength);

            var imageFileEntry0102 = image.Entries.Single(e => e.TagId == 0x0102 && e.TagType == 3);
            // Assert.Equal(3u, imageFileEntry0102.NumberOfValue);
            // Assert.Equal(238u, imageFileEntry0102.ValuePointer);
            var bitsPerSample = RawImage.ReadUInts16(binaryReader, imageFileEntry0102);
            Assert.Equal(new[] { (ushort)8, (ushort)8, (ushort)8 }, bitsPerSample);

            var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
            Assert.Equal(6u, compression);

            var imageFileEntry010F = image.Entries.Single(e => e.TagId == 0x010F && e.TagType == 2);
            // Assert.Equal(6u, imageFileEntry010F.NumberOfValue);
            // Assert.Equal(244u, imageFileEntry010F.ValuePointer);
            var make = RawImage.ReadChars(binaryReader, imageFileEntry010F);
            Assert.Equal("Canon", make);

            var imageFileEntry0110 = image.Entries.Single(e => e.TagId == 0x0110 && e.TagType == 2);
            // Assert.Equal(22u, imageFileEntry0110.NumberOfValue);
            // Assert.Equal(250u, imageFileEntry0110.ValuePointer);
            var model = RawImage.ReadChars(binaryReader, imageFileEntry0110);
            Assert.Equal("Canon EOS 5D Mark III", model);

            var stripOffset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
            // Assert.Equal(99812u, stripOffset);

            var orientation = image.Entries.Single(e => e.TagId == 0x0112 && e.TagType == 3).ValuePointer;
            // Assert.Equal(1u, orientation);    // 1 = 0,0 is top left

            var stripByteCounts = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
            // Assert.Equal(2823352u, stripByteCounts);

            var imageFileEntry011A = image.Entries.Single(e => e.TagId == 0x011A && e.TagType == 5);
            // Assert.Equal(1u, imageFileEntry011A.NumberOfValue);
            // Assert.Equal(282u, imageFileEntry011A.ValuePointer);
            var xResolution = RawImage.ReadRational(binaryReader, imageFileEntry011A);
            Assert.Equal(new[] { 72u, 1u }, xResolution);

            var imageFileEntry011B = image.Entries.Single(e => e.TagId == 0x011B && e.TagType == 5);
            // Assert.Equal(1u, imageFileEntry011B.NumberOfValue);
            // Assert.Equal(290u, imageFileEntry011B.ValuePointer);
            var yResolution = RawImage.ReadRational(binaryReader, imageFileEntry011B);
            Assert.Equal(new[] { 72u, 1u }, yResolution);

            var resolutionUnit = image.Entries.Single(e => e.TagId == 0x0128 && e.TagType == 3).ValuePointer;
            Assert.Equal(2u, resolutionUnit); // 1 == none, 2 == pixels per inch, 3 == pixels per centimeter

            var imageFileEntry0132 = image.Entries.Single(e => e.TagId == 0x0132 && e.TagType == 2);
            // Assert.Equal(20u, imageFileEntry0132.NumberOfValue);
            // Assert.Equal(298u, imageFileEntry0132.ValuePointer);
            var dateTime = RawImage.ReadChars(binaryReader, imageFileEntry0132);
            // Assert.Equal("2016:02:21 14:04:17", dateTime);

            var imageFileEntry013B = image.Entries.Single(e => e.TagId == 0x013B && e.TagType == 2);
            // Assert.Equal(11u, imageFileEntry013B.NumberOfValue);
            // Assert.Equal(318u, imageFileEntry013B.ValuePointer);
            var item13 = RawImage.ReadChars(binaryReader, imageFileEntry013B);
            Assert.Equal("Greg Eakin", item13);

            var imageFileEntry02BC = image.Entries.Single(e => e.TagId == 0x02BC && e.TagType == 1);
            // Assert.Equal(8192u, imageFileEntry02BC.NumberOfValue);
            // Assert.Equal(72132u, imageFileEntry02BC.ValuePointer);
            var xmpData = RawImage.ReadBytes(binaryReader, imageFileEntry02BC);
            var xmp = System.Text.Encoding.UTF8.GetString(xmpData);

            var imageFileEntry8298 = image.Entries.Single(e => e.TagId == 0x8298 && e.TagType == 2);
            // Assert.Equal(53u, imageFileEntry8298.NumberOfValue);
            // Assert.Equal(382u, imageFileEntry8298.ValuePointer);
            var item15 = RawImage.ReadChars(binaryReader, imageFileEntry8298);
            // Assert.Equal("Copyright (c) 2015, Greg Eakin. All rights reserved.", item15);

            var exif = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4).ValuePointer;
            // Assert.Equal(446u, exif);

            var gps = image.Entries.Single(e => e.TagId == 0x8825 && e.TagType == 4).ValuePointer;
            // Assert.Equal(70028u, gps);
        }
    }
}
