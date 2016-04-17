using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class ExifData
    {
        [TestMethod]
        public void DumpExifData1()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);
                exif.DumpDirectory(binaryReader);

                Assert.AreEqual(38, exif.Entries.Length);

                var exposeure = exif.Entries.Single(e => e.TagId == 0x829A && e.TagType == 5);
                var fStop = exif.Entries.Single(e => e.TagId == 0x829D && e.TagType == 5);
                var iso = exif.Entries.Single(e => e.TagId == 0x8827 && e.TagType == 3);

                // 0)  0x829A URational 2x32-bit: [0x0000038C] (1): 30/1 = 30   // exposeure time
                // 1)  0x829D URational 2x32-bit: [0x00000394] (1): 32/10 = 3.2 // f number
                // 2)  0x8822 UShort 16-bit: 3                                  // Exposure program
                // 3)  0x8827 UShort 16-bit: 100                                // ISO speed 100
                // 4)  0x8830 UShort 16-bit: 2                                  
                // 5)  0x8832 ULong 32-bit: 100
                // 6)  0x9000 UByte[]: 48, 50, 51, 48                           // Exif version 2.30
                // 7)  0x9003 Ascii 8-bit, null terminated: [0x0000039C] (20): "2014:03:31 06:17:21"    // date time, original
                // 8)  0x9004 Ascii 8-bit, null terminated: [0x000003B0] (20): "2014:03:31 06:17:21"    // date time, digitized
                // 9)  0x9101 UByte[]: 1, 2, 3, 0                                           // Componets configuration YCbCr
                //10)  0x9201 SRational 2x32-bit: [0x000003C4] (1): -327680/65536 = -5      // Shutter speed (32.00s)
                //11)  0x9202 URational 2x32-bit: [0x000003CC] (1): 221184/65536 = 3.375    // Appture value (F3.2)
                //12)  0x9204 SRational 2x32-bit: [0x000003D4] (1): 0/1 = 0                 // exposure bios
                //13)  0x9207 UShort 16-bit: 3                                              // metering mode (spot)
                //14)  0x9209 UShort 16-bit: 16                                             // flash (not fired)
                //15)  0x920A URational 2x32-bit: [0x000003DC] (1): 32/1 = 32               // focal length
                //16)  0x927C Maker note: [0x000003E4] (68540): 
                //17)  0x9286 UByte[]: [0x00010FA0] (264):                                  // user comment
                //18)  0x9290 Ascii 8-bit, null terminated: [0x00003030] (3): "00"          // subsecond time
                //19)  0x9291 Ascii 8-bit, null terminated: [0x00003030] (3): "00"          // subsecond time original
                //20)  0x9292 Ascii 8-bit, null terminated: [0x00003030] (3): "00"          // subsecond time digitized
                //21)  0xA000 UByte[]: 48, 49, 48, 48                                       // Flash Pix Version 0100
                //22)  0xA001 UShort 16-bit: 1                                              // colorspace sRGB
                //23)  0xA002 UShort 16-bit: 5760                                           // width
                //24)  0xA003 UShort 16-bit: 3840                                           // height
                //25)  0xA005 ULong 32-bit: 69800                                           // interoperability offset
                //26)  0xA20E URational 2x32-bit: [0x000110C6] (1): 5760000/1461 = 3942.50513347023     // focal plane X resolution 
                //27)  0xA20F URational 2x32-bit: [0x000110CE] (1): 3840000/972 = 3950.61728395062      // focal plane Y resolution
                //28)  0xA210 UShort 16-bit: 2                                                          // resolution units inches
                //29)  0xA401 UShort 16-bit: 0
                //30)  0xA402 UShort 16-bit: 0
                //31)  0xA403 UShort 16 - bit: 0
                //32)  0xA406 UShort 16 - bit: 0
                //33)  0xA430 Ascii 8 - bit, null terminated: [0x000110D6](11): "Greg Eakin"            // author
                //34)  0xA431 Ascii 8 - bit, null terminated: [0x000110F6](13): "032033000212"          // Camera serial number
                //35)  0xA432 URational 2x32 - bit: [0x00011116] (4): 24/1 = 24                         // focual length
                //36)  0xA434 Ascii 8-bit, null terminated: [0x00011136] (21): "EF24-70mm f/2.8L USM"   // lens
                //37)  0xA435 Ascii 8-bit, null terminated: [0x00011180] (11): "0000000000"

                var notesEntry = exif[0x927c];
                // Assert.AreEqual(68540u, notesEntry.NumberOfValue);
                // Assert.AreEqual(0x000001BEu, imageFileEntry.ValuePointer);

                var interopEntry = exif.Entries.Single(e => e.TagId == 0xA005 && e.TagType == 4);
                binaryReader.BaseStream.Seek(interopEntry.ValuePointer, SeekOrigin.Begin);
                var interop = new ImageFileDirectory(binaryReader);
                CollectionAssert.AreEqual(new ushort[] { 0x0001, 0x0002, }, interop.Entries.Select(e => e.TagId).ToArray());
                // interop.DumpDirectory(binaryReader);

                var tag01 = interop.Entries.Single(e => e.TagId == 0x0001 && e.TagType == 2);
                var index = RawImage.ReadChars(binaryReader, tag01);
                Assert.AreEqual("R98", index);

                var tag02 = interop.Entries.Single(e => e.TagId == 0x0002 && e.TagType == 7);
                var version = RawImage.ReadChars(binaryReader, tag02);
                Assert.AreEqual("0100", version);
            }
        }

        [TestMethod]
        public void DumpExifData2()
        {
            const string fileName = @"C:..\..\Photos\7DSraw.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);
                exif.DumpDirectory(binaryReader);

                Assert.AreEqual(37, exif.Entries.Length);

                var data1B = RawImage.ReadBytes(binaryReader, exif.Entries.Single(e => e.TagId == 0x9286 && e.TagType == 7));
                Assert.AreEqual(264, data1B.Length);
                foreach (var b in data1B)
                    Assert.AreEqual(0x00, b);

                // 0)  0x829A URational 2x32-bit: [0x00000380] (1): 1/200 = 0.005
                // 1)  0x829D URational 2x32-bit: [0x00000388] (1): 5/1 = 5
                // 2)  0x8822 UShort 16-bit: 2
                // 3)  0x8827 UShort 16-bit: 100
                // 4)  0x8830 UShort 16-bit: 2
                // 5)  0x8832 ULong 32-bit: 100
                // 6)  0x9000 UByte[]: 48, 50, 51, 48
                // 7)  0x9003 Ascii 8-bit, null terminated: [0x00000390] (20): "2016:02:26 14:03:33"
                // 8)  0x9004 Ascii 8-bit, null terminated: [0x000003A4] (20): "2016:02:26 14:03:33"
                // 9)  0x9101 UByte[]: 1, 2, 3, 0
                //10)  0x9201 SRational 2x32-bit: [0x000003B8] (1): 499712/65536 = 7.625
                //11)  0x9202 URational 2x32-bit: [0x000003C0] (1): 303104/65536 = 4.625
                //12)  0x9204 SRational 2x32-bit: [0x000003C8] (1): 0/1 = 0
                //13)  0x9207 UShort 16-bit: 5
                //14)  0x9209 UShort 16-bit: 16
                //15)  0x920A URational 2x32-bit: [0x000003D0] (1): 50/1 = 50
                //16)  0x927C Maker note: [0x000003D8] (45494): 
                //17)  0x9286 UByte[]: [0x0000B58E] (264): 
                //18)  0x9290 Ascii 8-bit, null terminated: [0x00003137] (3): "71"
                //19)  0x9291 Ascii 8-bit, null terminated: [0x00003137] (3): "71"
                //20)  0x9292 Ascii 8-bit, null terminated: [0x00003137] (3): "71"
                //21)  0xA000 UByte[]: 48, 49, 48, 48
                //22)  0xA001 UShort 16-bit: 1
                //23)  0xA002 UShort 16-bit: 2592
                //24)  0xA003 UShort 16-bit: 1728
                //25)  0xA005 ULong 32-bit: 46742
                //26)  0xA20E URational 2x32-bit: [0x0000B6B4] (1): 2592000/907 = 2857.77287761852
                //27)  0xA20F URational 2x32-bit: [0x0000B6BC] (1): 1728000/595 = 2904.20168067227
                //28)  0xA210 UShort 16-bit: 2
                //29)  0xA401 UShort 16-bit: 0
                //30)  0xA402 UShort 16-bit: 0
                //31)  0xA403 UShort 16-bit: 1
                //32)  0xA406 UShort 16-bit: 0
                //33)  0xA430 Ascii 8-bit, null terminated: [0x0000B6C4] (11): "Greg Eakin"
                //34)  0xA431 Ascii 8-bit, null terminated: [0x0000B6E4] (11): "3071201378"
                //35)  0xA432 URational 2x32-bit: [0x0000B704] (4): 50/1 = 50
                //36)  0xA434 Ascii 8-bit, null terminated: [0x0000B724] (17): "EF50mm f/1.4 USM"
            }
        }

        [TestMethod]
        public void DumpExifData3()
        {
            const string fileName = @"C:..\..\Photos\7Dhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);
                exif.DumpDirectory(binaryReader);

                Assert.AreEqual(37, exif.Entries.Length);

                // 0)  0x829A URational 2x32-bit: [0x00000380] (1): 1/15 = 0.0666666666666667
                // 1)  0x829D URational 2x32-bit: [0x00000388] (1): 18/10 = 1.8
                // 2)  0x8822 UShort 16-bit: 2
                // 3)  0x8827 UShort 16-bit: 400
                // 4)  0x8830 UShort 16-bit: 2
                // 5)  0x8832 ULong 32-bit: 400
                // 6)  0x9000 UByte[]: 48, 50, 51, 48
                // 7)  0x9003 Ascii 8-bit, null terminated: [0x00000390] (20): "2012:11:18 19:04:22"
                // 8)  0x9004 Ascii 8-bit, null terminated: [0x000003A4] (20): "2012:11:18 19:04:22"
                // 9)  0x9101 UByte[]: 1, 2, 3, 0
                //10)  0x9201 SRational 2x32-bit: [0x000003B8] (1): 262144/65536 = 4
                //11)  0x9202 URational 2x32-bit: [0x000003C0] (1): 106496/65536 = 1.625
                //12)  0x9204 SRational 2x32-bit: [0x000003C8] (1): 0/1 = 0
                //13)  0x9207 UShort 16-bit: 5
                //14)  0x9209 UShort 16-bit: 16
                //15)  0x920A URational 2x32-bit: [0x000003D0] (1): 50/1 = 50
                //16)  0x927C Maker note: [0x000003D8] (45494): 
                //17)  0x9286 UByte[]: [0x0000B58E] (264): 
                //18)  0x9290 Ascii 8-bit, null terminated: [0x00003030] (3): "00"
                //19)  0x9291 Ascii 8-bit, null terminated: [0x00003030] (3): "00"
                //20)  0x9292 Ascii 8-bit, null terminated: [0x00003030] (3): "00"
                //21)  0xA000 UByte[]: 48, 49, 48, 48
                //22)  0xA001 UShort 16-bit: 1
                //23)  0xA002 UShort 16-bit: 5184
                //24)  0xA003 UShort 16-bit: 3456
                //25)  0xA005 ULong 32-bit: 46742
                //26)  0xA20E URational 2x32-bit: [0x0000B6B4] (1): 5184000/907 = 5715.54575523705
                //27)  0xA20F URational 2x32-bit: [0x0000B6BC] (1): 3456000/595 = 5808.40336134454
                //28)  0xA210 UShort 16-bit: 2
                //29)  0xA401 UShort 16-bit: 0
                //30)  0xA402 UShort 16-bit: 0
                //31)  0xA403 UShort 16-bit: 0
                //32)  0xA406 UShort 16-bit: 0
                //33)  0xA430 Ascii 8-bit, null terminated: [0x0000B6C4] (11): "Greg Eakin"
                //34)  0xA431 Ascii 8-bit, null terminated: [0x0000B6E4] (11): "3071201378"
                //35)  0xA432 URational 2x32-bit: [0x0000B704] (4): 50/1 = 50
                //36)  0xA434 Ascii 8-bit, null terminated: [0x0000B724] (16): "EF50mm f/1.8 II"
            }
        }
    }
}

