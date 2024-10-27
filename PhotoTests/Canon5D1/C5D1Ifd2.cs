//    Copyright 2022 Gregory Eakin
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;
using Xunit.Abstractions;

namespace PhotoTests.Canon5D1;

public class C5D1Ifd2
{
    private const string FileName = @"P:\2022\2022-08-03\IMG_0256.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C5D1Ifd2(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    [Fact]
    public void DumpImageFileDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Skip(2).First();

        var directory = imageFileDirectory.DumpDirectory(binaryReader);
        _testOutputHelper.WriteLine(directory);
    }

    // == Tiff Directory[0x00012958]:
    // ImageWidth                   IFD2:0x0100  0)  UShort 16-bit: 384
    // ImageLength                  IFD2:0x0101  1)  UShort 16-bit: 256
    // BitsPerSample                IFD2:0x0102  2)  UShort 16-bit: [0x000129E2] (3): 8, 8, 8, 
    // Compression                  IFD2:0x0103  3)  UShort 16-bit: 6
    // PhotometricInterpretation    IFD2:0x0106  4)  UShort 16-bit: 2
    // StripOffsets                 IFD2:0x0111  5)  ULong 32-bit: 1739602
    // SamplesPerPixel              IFD2:0x0115  6)  UShort 16-bit: 3
    // RowsPerStrip                 IFD2:0x0116  7)  UShort 16-bit: 256
    // StripByteCounts              IFD2:0x0117  8)  ULong 32-bit: 294912
    // PlanarConfiguration          IFD2:0x011C  9)  UShort 16-bit: 1
    //                              IFD2:0xC5D9 10)  ULong 32-bit: 2

    [Fact]
    public void TagsInDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Skip(2).First();

        Assert.Equal(
            new ushort[] { 0x0100, 0x0101, 0x0102, 0x0103, 0x0106, 0x0111, 0x0115, 0x0116, 0x0117, 0x011C, 0xC5D9 },
            imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }
}
