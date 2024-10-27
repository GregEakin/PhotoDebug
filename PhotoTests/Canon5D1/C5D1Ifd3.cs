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

public class C5D1Ifd3
{
    private const string FileName = @"P:\2022\2022-08-03\IMG_0256.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C5D1Ifd3(ITestOutputHelper testOutputHelper)
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
        var imageFileDirectory = rawImage.Directories.Skip(3).First();

        var directory = imageFileDirectory.DumpDirectory(binaryReader);
        _testOutputHelper.WriteLine(directory);
    }

    // == Tiff Directory[0x000129E8]:
    // Compression              IFD3:0x0103  0)  UShort 16-bit: 6
    // StripOffsets             IFD3:0x0111  1)  ULong 32-bit: 2034514
    // StripByteCounts          IFD3:0x0117  2)  ULong 32-bit: 10885827
    //                          IFD3:0xC5D8  3)  ULong 32-bit: 1
    // CR2 CFA Pattern          IFD3:0xC5E0  4)  ULong 32-bit: 1
    // Raw Image Segmentation   IFD3:0xC640  5)  UShort 16-bit: [0x00012A36] (3): 1, 2238, 2238, 

    [Fact]
    public void TagsInDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Skip(3).First();

        Assert.Equal(
            new ushort[] { 0x0103, 0x0111, 0x0117, 0xC5D8, 0xC5E0, 0xC640 },
            imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }
}
