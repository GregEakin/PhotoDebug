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
using Xunit;
using Xunit.Abstractions;

namespace PhotoTests.CanonR;

public class C0RIfd0
{
    private const string FileName = @"\\Data\Photo\2019\2019-09-02\IMG_0001.CR3";
    private readonly ITestOutputHelper _testOutputHelper;

    public C0RIfd0(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    [Fact]
    public void DumpHeader()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BigEndianBinaryReader(fileStream);
        // var rawImage = new RawImage(binaryReader);
        // Assert.Equal(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
        // Assert.Equal(0x002A, rawImage.Header.TiffMagic);
        // Assert.Equal(0x5243, rawImage.Header.CR2Magic);
        // Assert.Equal(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);
        //
        // var header = rawImage.DumpHeader(binaryReader);
        // _testOutputHelper.WriteLine(header);
    }

    [Fact]
    public void DumpImageFileDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BigEndianBinaryReader(fileStream);
        // var rawImage = new RawImage(binaryReader);
        // var imageFileDirectory = rawImage.Directories.First();
        //
        // var directory = imageFileDirectory.DumpDirectory(binaryReader);
        // _testOutputHelper.WriteLine(directory);
    }

    [Fact]
    public void TagsInDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BigEndianBinaryReader(fileStream);
        // var rawImage = new RawImage(binaryReader);
        // var imageFileDirectory = rawImage.Directories.First();
        //
        // Assert.Equal(
        //     new ushort[] { 0x0100, 0x0101, 0x0102, 0x0103, 0x010F, 0x0110, 0x0111, 0x0112, 0x0117, 0x011A, 0x011B, 0x0128, 0x0132, 0x8769 },
        //     imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }

}
