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
using Xunit.Abstractions;

namespace PhotoTests.Canon1D3;

public class C1D3
{
    private const string FileName = @"\\Data\Photo\2024\2024-08-31\2B0I7592.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C1D3(ITestOutputHelper testOutputHelper)
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
    // ImageWidth       IFD0:0x0100  0)  UShort 16-bit: 1936
    // ImageLength      IFD0:0x0101  1)  UShort 16-bit: 1288
    // BitsPerSample    IFD0:0x0102  2)  UShort 16-bit: [0x000000CA] (3): 8, 8, 8, 
    // Compression      IFD0:0x0103  3)  UShort 16-bit: 6
    // Make             IFD0:0x010F  4)  Ascii 8-bit, null terminated: [0x000000D0] (6): "Canon"
    // Model            IFD0:0x0110  5)  Ascii 8-bit, null terminated: [0x000000D6] (13): "Canon EOS-1D Mark III"
    // StripOffsets     IFD0:0x0111  6)  ULong 32-bit: 50924
    // Orientation      IFD0:0x0112  7)  UShort 16-bit: 1
    // StripByteCounts  IFD0:0x0117  8)  ULong 32-bit: 787019
    // XResolution      IFD0:0x011A  9)  URational 2x32-bit: [0x000000F6] (1): 72/1 = 72
    // YResolution      IFD0:0x011B 10)  URational 2x32-bit: [0x000000FE] (1): 72/1 = 72
    // ResolutionUnit   IFD0:0x0128 11)  UShort 16-bit: 2
    // DateTime         IFD0:0x0132 12)  Ascii 8-bit, null terminated: [0x00000106] (20): "2024:08:31 09:04:04"
    // Exif IFD Pointer IFD0:0x8769 13)  Image File Directory: [0x0000011A] (1): 

    // == Tiff Directory[0x0001293A]:
    // Thumbnail Offset IFD1:0x0201  0)  ULong 32-bit: 76348
    // Thumbnail Length IFD1:0x0202  1)  ULong 32-bit: 11256

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

    // == Tiff Directory[0x000129E8]:
    // Compression              IFD3:0x0103  0)  UShort 16-bit: 6
    // StripOffsets             IFD3:0x0111  1)  ULong 32-bit: 2034514
    // StripByteCounts          IFD3:0x0117  2)  ULong 32-bit: 10885827
    //                          IFD3:0xC5D8  3)  ULong 32-bit: 1
    // CR2 CFA Pattern          IFD3:0xC5E0  4)  ULong 32-bit: 1
    // Raw Image Segmentation   IFD3:0xC640  5)  UShort 16-bit: [0x00012A36] (3): 1, 2238, 2238, 
}
