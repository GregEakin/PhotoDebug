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

public class C5D1Ifd1
{
    private const string FileName = @"P:\2022\2022-08-03\IMG_0256.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C5D1Ifd1(ITestOutputHelper testOutputHelper)
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
        var imageFileDirectory = rawImage.Directories.Skip(1).First();

        var directory = imageFileDirectory.DumpDirectory(binaryReader);
        _testOutputHelper.WriteLine(directory);
    }

    // == Tiff Directory[0x0001293A]:
    // Thumbnail Offset IFD1:0x0201  0)  ULong 32-bit: 76348
    // Thumbnail Length IFD1:0x0202  1)  ULong 32-bit: 11256

    [Fact]
    public void TagsInDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Skip(1).First();

        Assert.Equal(
            new ushort[] { 0x0201, 0x0202 },
            imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }
}
