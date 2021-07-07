// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		KeyInformation.cs
// AUTHOR:		Greg Eakin

using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;
using Xunit;

namespace PhotoTests.Prototypes
{
    public class KeyInformation
    {
        [Fact]
        public void DumpKeyInformation()
        {
            const string fileName = @"P:\Source\5DIIIhigh.CR2";
            using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            //From IFD#0:
            var image = rawImage.Directories.First();

            //Camera make is taken from tag #271 (0x10f)
            var make = RawImage.ReadChars(binaryReader, image[0x010F]);
            Assert.Equal("Canon", make);

            //Camera model is from tag #272 (0x110)
            var model = RawImage.ReadChars(binaryReader, image[0x0110]);
            Assert.Equal("Canon EOS 5D Mark III", model);

            var exifEntry = image[0x8769];
            binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
            var exif = new ImageFileDirectory(binaryReader);

            var notesEntry = exif[0x927c];
            binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
            var notes = new ImageFileDirectory(binaryReader);

            //model ID from Makernotes, Tag #0x10
            var modelId = notes[0x0010];
            Assert.Equal(0x80000285, modelId.ValuePointer);

            //white balance information is taken from tag #0x4001
            var white = notes[0x4001];

            //From IFD#3:
            image = rawImage.Directories.Skip(3).First();

            //StripOffset, offset to RAW data : tag #0x111
            var offset = image[0x0111].ValuePointer;

            //StripByteCount, length of RAW data: tag #0x117
            var count = image[0x0117].ValuePointer;

            //image slice layout (cr2_slice[]) : tag #0xc640
            var imageFileEntry = image[0xC640];
            Assert.Equal(3u, imageFileEntry.NumberOfValue);
            var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
            Assert.Equal(new ushort[] {1, 2960, 2960}, slices);

            //the RAW image dimensions is taken from lossless jpeg (0xffc3 section)
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var startOfImage = new StartOfImage(binaryReader, offset, count);

            var startOfFrame = startOfImage.StartOfFrame;
            Assert.Equal(3950u, startOfFrame.ScanLines); // = 3840 + 110
            Assert.Equal(2960u, startOfFrame.SamplesPerLine); // = 5920 / 2
            Assert.Equal(2, startOfFrame.Components.Length);
            Assert.Equal(5920, startOfFrame.Width); // = 5760 + 160
        }
    }
}
