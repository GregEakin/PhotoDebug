//    Copyright 2024 Gregory Eakin
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

namespace PhotoTests.Canon1DX;
public class C1DX
{
    private const string FileName = @"P:\2024\2024-09-10\B35V5694.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C1DX(ITestOutputHelper testOutputHelper)
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
    // Image Width                  : IFD0:0x0100  0)  UShort 16-bit: 5184
    // Image Height                 : IFD0:0x0101  1)  UShort 16-bit: 3456
    // Bits Per Sample              : IFD0:0x0102  2)  UShort 16-bit: [0x000000EE] (3): 8, 8, 8, 
    // Compression                  : IFD0:0x0103  3)  UShort 16-bit: 6 JPEG (old-style)
    // Make                         : IFD0:0x010F  4)  Ascii 8-bit, null terminated: [0x000000F4] (6): "Canon"
    // Camera Model Name            : IFD0:0x0110  5)  Ascii 8-bit, null terminated: [0x000000FA] (15): "Canon EOS-1D X"
    // Preview Image Start          : IFD0:0x0111  6)  ULong 32-bit: 89440
    // Orientation                  : IFD0:0x0112  7)  UShort 16-bit: 1 Horizontal (normal)
    // Preview Image Length         : IFD0:0x0117  8)  ULong 32-bit: 733155
    // X Resolution                 : IFD0:0x011A  9)  URational 2x32-bit: [0x0000011A] (1): 72/1 = 72
    // Y Resolution                 : IFD0:0x011B 10)  URational 2x32-bit: [0x00000122] (1): 72/1 = 72
    // Resolution Unit              : IFD0:0x0128 11)  UShort 16-bit: 2 inches
    // Modify Date                  : IFD0:0x0132 12)  Ascii 8-bit, null terminated: [0x0000012A] (20): "2024:09:10 03:03:55"
    // Artist                       : IFD0:0x013B 13)  Ascii 8-bit, null terminated: [0x0000013E] (14): "Gregory Eakin"
    // Rating                       : IFD0:0x02BC 14)  XMP metadata: [0x00011A00] (8192): 
    // Copyright                    : IFD0:0x8298 15)  Ascii 8-bit, null terminated: [0x0000017E] (55): "Copyright (c) 2024 Gregory Eakin, all rights reserved."
    //                              : IFD0:0x8769 16)  Image File Directory: [0x000001BE] (1):     

    // == Tiff Directory[0x000118CA]:
    // Thumbnail Offset             : IFD1:0x0201  0)  ULong 32-bit: 80384
    // Thumbnail Length             : IFD1:0x0202  1)  ULong 32-bit: 9056

    // == Tiff Directory[0x000118E8]:
    // IFD2:0x0100  0)  UShort 16-bit: 668
    // IFD2:0x0101  1)  UShort 16-bit: 448
    // IFD2:0x0102  2)  UShort 16-bit: [0x0001198A] (3): 16, 16, 16, 
    // IFD2:0x0103  3)  UShort 16-bit: 1
    // IFD2:0x0106  4)  UShort 16-bit: 2
    // IFD2:0x0111  5)  ULong 32-bit: 822596
    // IFD2:0x0115  6)  UShort 16-bit: 3
    // IFD2:0x0116  7)  UShort 16-bit: 448
    // IFD2:0x0117  8)  ULong 32-bit: 1795584
    // IFD2:0x011C  9)  UShort 16-bit: 1
    // IFD2:0xC5D9 10)  ULong 32-bit: 2
    // IFD2:0xC6C5 11)  ULong 32-bit: 3
    // IFD2:0xC6DC 12)  ULong 32-bit: [0x00011990] (4): 0285 01AE 0013 000F 

    // == Tiff Directory[0x000119A0]:
    // IFD3:0x0103  0)  UShort 16-bit: 6
    // IFD3:0x0111  1)  ULong 32-bit: 2623556
    // IFD3:0x0117  2)  ULong 32-bit: 17270812
    // IFD3:0xC5D8  3)  ULong 32-bit: 1
    // IFD3:0xC5E0  4)  ULong 32-bit: 1
    // IFD3:0xC640  5)  UShort 16-bit: [0x000119FA] (3): 1, 2672, 2672, 
    // IFD3:0xC6C5  6)  ULong 32-bit: 1
}
