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
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Canon1D4;

public class C1D4
{
    private const string FileName = @"P:\2024\2024-09-02\BZ5F3119.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C1D4(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    [Fact]
    public void DumpHeader()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        Assert.Equal(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
        Assert.Equal(0x002A, rawImage.Header.TiffMagic);
        Assert.Equal(0x5243, rawImage.Header.CR2Magic);
        Assert.Equal(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

        var header = rawImage.DumpHeader(binaryReader);
        _testOutputHelper.WriteLine(header);
    }

    // == Tiff Directory[0x00000010]:
    // IFD0:0x0100  0)  UShort 16-bit: 4896
    // IFD0:0x0101  1)  UShort 16-bit: 3264
    // IFD0:0x0102  2)  UShort 16-bit: [0x000000E2] (3): 8, 8, 8, 
    // IFD0:0x0103  3)  UShort 16-bit: 6
    // IFD0:0x010F  4)  Ascii 8-bit, null terminated: [0x000000E8] (6): "Canon"
    // IFD0:0x0110  5)  Ascii 8-bit, null terminated: [0x000000EE] (21): "Canon EOS-1D Mark IV"
    // IFD0:0x0111  6)  ULong 32-bit: 60152
    // IFD0:0x0112  7)  UShort 16-bit: 1
    // IFD0:0x0117  8)  ULong 32-bit: 908896
    // IFD0:0x011A  9)  URational 2x32-bit: [0x0000010E] (1): 72/1 = 72
    // IFD0:0x011B 10)  URational 2x32-bit: [0x00000116] (1): 72/1 = 72
    // IFD0:0x0128 11)  UShort 16-bit: 2
    // IFD0:0x0132 12)  Ascii 8-bit, null terminated: [0x0000011E] (20): "2024:09:02 16:33:54"
    // IFD0:0x013B 13)  Ascii 8-bit, null terminated: [0x00000132] (14): "Gregory Eakin"
    // IFD0:0x8298 14)  Ascii 8-bit, null terminated: [0x00000000] (1): ""
    // IFD0:0x8769 15)  Image File Directory: [0x000001B2] (1): 
}
