// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		MakerNoteTests.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class MakerNoteTests
    {
        [TestMethod]
        public void DumpMakerNotes1()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);
                Assert.AreEqual(42, notes.Entries.Length);

                notes.DumpDirectory(binaryReader);

                // camera settings notes[0x0001]
                // focus info notes[0x0002]
                // image type notes[0x0006]
                // dust delete notes[0x0097]
                // color balance notes[0x4001]
                // AF Micro adjust notes[0x4013]
                // Vignetting correction notes[0x40015]

                var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                Assert.AreEqual("Canon EOS 5D Mark III", model);

                var firmware = RawImage.ReadChars(binaryReader, notes[0x0007]);
                Assert.AreEqual("Firmware Version 1.2.3\0", firmware);

                // 0x0010 ULong 32 - bit: 2147484293
                var id = notes.Entries.Single(e => e.TagId == 0x0010 && e.TagType == 4);
                Assert.AreEqual(0x80000285, id.ValuePointer);

                // 0)  0x0001 UShort 16-bit: [0x000005E2] (49): 98, 2, 0, 4, 0, 0, 0, 3, 0, 6, 65535, 1, 0, 0, 0, 32767, 32767, 1, 2, 0, 3, 65535, 230, 70, 24, 1, 96, 288, 0, 0, 0, 0, 65535, 65535, 65535, 0, 0, 0, 0, 65535, 65535, 0, 0, 32767, 65535, 65535, 0, 0, 65535, 
                //              00: length, bytes
                //              01: Macro mode, 2 == Normal
                //              03: RAW
                //              05: drive single
                //              07: Focus one shot
                //              09: Record mode, 6 == CR2
                //              0a: image size, -1 == N/A
                //              0b: Program manual
                //              10: ISO 16383
                //              17: Lens 24-70
                // 1)  0x0002 UShort 16-bit: [0x00000644] (4): 0, 32, 53893, 12236, 
                // 2)  0x0003 UShort 16-bit: [0x0000064C] (4): 0, 0, 0, 0, 
                // 3)  0x0004 UShort 16-bit: [0x00000654] (34): 68, 0, 160, 65324, 108, 65376, 0, 0, 3, 0, 8, 8, 148, 0, 0, 0, 0, 0, 1, 0, 0, 108, 65376, 45, 0, 0, 248, 65535, 65535, 65535, 65535, 0, 0, 0, 
                //              07: white balance auto
                //              09: Sequence number
                //              0f: flash bias
                // 4)  0x0006 Ascii 8-bit, null terminated: [0x00000698] (22): "Canon EOS 5D Mark III"
                // 5)  0x0007 Ascii 8-bit, null terminated: [0x000006B8] (24): "Firmware Version 1.2.3"
                // 6)  0x0009 Ascii 8-bit, null terminated: [0x000006D0] (32): "Greg Eakin"
                // 7)  0x000D UByte[]: [0x000006F0] (1536):                     // camera info
                // 8)  0x0010 ULong 32-bit: 2147484293                          // camera id
                // 9)  0x0013 UShort 16-bit: [0x00000CF0] (4): 0, 159, 7, 112,  // thumbnail image valid area
                //10)  0x0019 UShort 16-bit: 1
                //11)  0x0026 UShort 16-bit: [0x00000CF8] (265): 530, 0, 61, 61, 5760, 3840, 5760, 3840, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 190, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 288, 0, 65248, 1520, 1232, 944, 639, 288, 0, 64016, 64304, 64592, 64897, 65248, 1520, 1232, 944, 639, 288, 0, 64016, 64304, 64592, 64897, 65248, 1520, 1232, 944, 639, 288, 0, 64016, 64304, 64592, 64897, 65248, 1520, 1232, 944, 639, 288, 0, 64016, 64304, 64592, 64897, 65248, 1520, 1232, 944, 639, 288, 0, 64016, 64304, 64592, 64897, 65248, 288, 0, 65248, 64888, 64888, 64888, 65103, 65103, 65103, 65103, 65103, 65103, 65103, 65103, 65103, 65103, 65103, 65321, 65321, 65321, 65321, 65321, 65321, 65321, 65321, 65321, 65321, 65321, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 215, 215, 215, 215, 215, 215, 215, 215, 215, 215, 215, 433, 433, 433, 433, 433, 433, 433, 433, 433, 433, 433, 648, 648, 648, 0, 0, 0, 0, 0, 0, 16384, 0, 0, 0, 0, 0, 65535, 
                //12)  0x0035 ULong 32-bit: [0x00000F0A] (4): 0010 FFFFFE5C 001E 003C    // time info
                //13)  0x0093 UShort 16-bit: [0x00000F1A] (30): 60, 0, 0, 0, 0, 0, 0, 0, 65535, 0, 0, 0, 0, 0, 65535, 65535, 90, 65535, 0, 0, 1191, 781, 0, 0, 0, 0, 65535, 0, 0, 0, 
                //14)  0x0095 Ascii 8-bit, null terminated: [0x00000F56] (74): "EF24-70mm f/2.8L USM"
                //15)  0x0096 Ascii 8-bit, null terminated: [0x00000FA0] (16): "AD0144782"
                //16)  0x0097 UByte[]: [0x00000FB0] (1024):                     // dust removal info
                //17)  0x0098 UShort 16-bit: [0x000013B0] (4): 0, 0, 0, 0,      // crop info
                //18)  0x0099 ULong 32-bit: [0x000013B8] (83): 014C 0003 0001 0054 0006 0101 0001 0000 0102 0001 0001 0104 0001 0000 0105 0001 0000 0106 0002 0003 0000 0108 0001 0000 0002 0020 0002 040A 0001 0007 040B 0001 0001 0004 00C4 0005 070C 0020 0000 0000 0000 0000 0001 0000 0001 0003 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0007 0003 0706 0001 0000 070F 0002 0000 0002 080E 0001 0000 0813 0001 0000 
                //19)  0x009A ULong 32-bit: [0x00001504] (5): 0000 1680 0F00 0000 0000  // aspect info
                //20)  0x00A0 UShort 16-bit: [0x00001518] (14): 28, 0, 0, 0, 0, 0, 0, 0, 65535, 5200, 133, 0, 0, 0,     //processing info 
                //21)  0x00AA UShort 16-bit: [0x00001534] (6): 12, 1057, 1024, 1024, 386, 0,            // measured color
                //22)  0x00B4 UShort 16-bit: 1                                  // color space, 1 == sRGB
                //23)  0x00D0 ULong 32-bit: 0                                   // VRD recipe
                //24)  0x00E0 UShort 16-bit: [0x00001540] (17): 34, 5920, 3950, 1, 1, 140, 96, 5899, 3935, 0, 0, 0, 0, 0, 0, 0, 0,      // sensor info 
                //25)  0x4001 UShort 16-bit: [0x00001562] (1312): 10, 819, 1024, 1024, 350, 570, 1024, 1024, 479, 398, 1024, 1024, 671, 1459, 1787, 1787, 609, 1557, 2724, 2725, 1263, 739, 1838, 1837, 1192, 3, 0, 263, 262, 265, 0, 1439, 3054, 3051, 1706, 665, 208, 207, 30, 75, 588, 589, 1011, 1365, 2358, 2358, 441, 1255, 2540, 2539, 1379, 577, 185, 185, 26, 67, 478, 478, 803, 1191, 1965, 1966, 367, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 1024, 1024, 1024, 1024, 4378, 1024, 1024, 1024, 1024, 4378, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 2032, 1024, 1024, 1702, 4997, 1370, 1028, 1018, 1952, 3415, 777, 1170, 1170, 529, 3415, 2076, 1024, 1024, 1657, 5200, 2383, 1024, 1024, 1411, 7000, 2231, 1024, 1024, 1524, 6000, 1494, 1024, 1024, 2473, 3200, 1824, 1024, 1024, 2378, 3714, 2076, 1024, 1024, 1657, 5189, 2315, 1024, 1024, 1500, 6320, 2068, 1024, 1024, 1446, 5940, 2076, 1024, 1024, 1657, 5189, 2076, 1024, 1024, 1657, 5189, 2076, 1024, 1024, 1657, 5189, 2076, 1024, 1024, 1657, 5189, 1010, 1024, 1024, 1022, 4325, 1010, 1024, 1024, 1022, 4325, 1010, 1024, 1024, 1022, 4325, 1010, 1024, 1024, 1022, 4325, 1010, 1024, 1024, 1022, 4325, 65228, 383, 881, 10900, 65246, 391, 858, 10000, 65292, 413, 800, 8300, 65343, 440, 743, 7000, 65396, 470, 688, 6000, 65423, 486, 662, 5600, 65453, 505, 633, 5200, 65500, 531, 586, 4700, 20, 570, 539, 4200, 74, 612, 498, 3800, 123, 652, 463, 3500, 180, 702, 424, 3200, 226, 741, 390, 3000, 267, 790, 371, 2800, 372, 920, 322, 2400, 500, 2065, 2081, 2048, 2048, 2048, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 409, 494, 708, 3748, 1676, 1764, 1587, 932, 5697, 4163, 4172, 2546, 1182, 935, 2342, 200, 64, 17, 18, 669, 49, 20, 37, 10, 399, 469, 1094, 1564, 593, 474, 2860, 597, 0, 1, 2, 8, 1, 2, 4, 0, 8, 6, 8, 4, 6, 1, 40, 31, 5, 7, 11, 46, 8, 7, 11, 3, 14, 8, 15, 16, 9, 5, 114, 47, 0, 0, 0, 32768, 0, 1024, 1024, 1024, 2639, 3807, 6471, 4116, 65511, 51, 4099, 4076, 24, 65485, 4093, 0, 256, 1, 37283, 1, 33091, 1, 31641, 0, 37046, 1024, 1024, 1024, 0, 0, 0, 65533, 0, 8191, 256, 0, 0, 1024, 686, 427, 491, 638, 406, 792, 0, 0, 0, 0, 0, 15, 240, 256, 256, 256, 256, 256, 256, 0, 15, 240, 256, 256, 256, 256, 256, 256, 0, 0, 131, 0, 16, 32, 64, 96, 128, 192, 0, 0, 0, 0, 0, 0, 0, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1160, 0, 2048, 2048, 2048, 2048, 14708, 15220, 10087, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 240, 256, 256, 256, 256, 256, 256, 0, 15, 240, 256, 256, 256, 256, 256, 256, 125, 125, 126, 1, 1, 244, 244, 8, 24, 60, 92, 111, 130, 166, 218, 235, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 35, 81, 98, 118, 133, 216, 219, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1391, 1024, 859, 0, 0, 0, 0, 75, 80, 32344, 525, 0, 0, 0, 0, 782, 225, 0, 575, 177, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 516, 1024, 1024, 617, 100, 0, 79, 79, 32331, 598, 0, 0, 0, 0, 100, 44, 72, 16, 33, 230, 255, 21305, 2068, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 31, 63, 95, 127, 159, 191, 223, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 126, 1, 244, 0, 0, 0, 0, 0, 0, 0, 31, 63, 95, 127, 159, 191, 223, 255, 48, 51, 55, 56, 54, 49, 46, 48, 54, 60, 68, 74, 82, 87, 90, 92, 94, 94, 91, 89, 86, 85, 88, 92, 94, 93, 94, 99, 104, 112, 113, 112, 111, 113, 118, 121, 124, 129, 132, 144, 154, 164, 189, 217, 240, 238, 235, 226, 212, 197, 168, 138, 97, 73, 64, 53, 47, 45, 46, 48, 47, 90, 85, 0, 255, 4, 44, 72, 21305, 2068, 0, 0, 0, 0, 0, 230, 255, 33, 823, 191, 0, 510, 136, 0, 352, 0, 0, 16, 0, 0, 0, 0, 0, 0, 65535, 0, 65535, 65535, 65535, 0, 0, 0, 0, 0, 0, 0, 0, 100, 100, 100, 100, 100, 100, 100, 100, 0, 100, 100, 0, 40, 0, 0, 0, 0, 0, 44, 72, 21305, 2068, 0, 0, 0, 0, 0, 230, 255, 33, 32331, 598, 0, 0, 0, 0, 21305, 2068, 4, 44, 0, 0, 0, 0, 0, 0, 0, 0, 31, 63, 95, 127, 159, 191, 223, 255, 0, 0, 516, 1024, 1024, 617, 
                //26)  0x4002 UByte[]: [0x00001FA2] (43572): 
                //27)  0x4005 UByte[]: [0x0000C9D6] (16792): 
                //28)  0x4008 UShort 16-bit: [0x00010B6E] (3): 129, 129, 129,       // black level
                //29)  0x4009 UShort 16-bit: [0x00010B74] (3): 0, 0, 0, 
                //30)  0x4010 Ascii 8-bit, null terminated: [0x00010B7A] (32): ""
                //31)  0x4011 UByte[]: [0x00010B9A] (252): 
                //32)  0x4012 Ascii 8-bit, null terminated: [0x00010C96] (32): ""
                //33)  0x4013 ULong 32-bit: [0x00010CB6] (11): 002C 0000 0000 000A FFFFFFFF 0000 000A 0000 000A 0000 000A       // AF micro adjust
                //34)  0x4015 UByte[]: [0x00010CE2] (456):                          // Vignetting Correction 
                //35)  0x4016 ULong 32-bit: [0x00010EAA] (7): 001C 0000 0001 0000 0000 0001 0001    // Vignetting Correction 2
                //36)  0x4018 ULong 32-bit: [0x00010EC6] (7): 001C 0000 0003 0000 0000 0003 0001    // Lighting Option
                //37)  0x4019 UByte[]: [0x00010EE2] (30):                                           // Lens info
                //38)  0x4021 ULong 32-bit: [0x00010F00] (5): 0014 0000 0000 0000 0001              // multi exposure
                //39)  0x4025 ULong 32-bit: [0x00010F14] (9): 0024 0000 0000 0000 0000 0000 0000 0000 0000      // HDR info
                //40)  0x4027 ULong 32-bit: [0x00010F38] (5): 0014 90001 A6A30034 73C0600 D0D0D0 
                //41)  0x4028 ULong 32-bit: [0x00010F4C] (19): 004C 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0001 003F 0000 0001 0001 0002 0000 0000   // AF Coding

                // Color Balance
                //var data1B = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4001 && e.TagType == 3));

                // Vignetting Correction
                //var data1B = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4015 && e.TagType == 7));

                // CRW Parm
                var data01 = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4002 && e.TagType == 7));

                // Flavor
                var data02 = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4005 && e.TagType == 7));

                var data03 = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4011 && e.TagType == 7));

                // Vignetting Correction
                var data04 = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4015 && e.TagType == 7));

                // Lens info
                var data05 = RawImage.ReadBytes(binaryReader, notes.Entries.Single(e => e.TagId == 0x4019 && e.TagType == 7));
            }
        }

        [TestMethod]
        public void DumpCameraSettings()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // 0)  0x0001 UShort 16-bit: [0x000005E2] (49): 98, 2, 0, 4, 0, 0, 0, 3, 0, 6, 65535, 1, 0, 0, 0, 32767, 32767, 1, 2, 0, 3, 65535, 230, 70, 24, 1, 96, 288, 0, 0, 0, 0, 65535, 65535, 65535, 0, 0, 0, 0, 65535, 65535, 0, 0, 32767, 65535, 65535, 0, 0, 65535, 
                var data = RawImage.ReadUInts16(binaryReader, notes.Entries.Single(e => e.TagId == 0x0001 && e.TagType == 3));

                Assert.AreEqual(2 * data.Length, data[0]);
                Assert.AreEqual(2, data[1]);                // 01: Macro mode, 2 == Normal
                Assert.AreEqual(4, data[3]);                // 03: quality, 4 == RAW
                Assert.AreEqual(0, data[5]);                // 05: drive single
                Assert.AreEqual(3, data[7]);                // 07: Focus one shot
                Assert.AreEqual(6, data[9]);                // 09: Record mode, 6 == CR2
                Assert.AreEqual(65535, data[10]);           // 0a: image size, -1 == N/A
                Assert.AreEqual(1, data[11]);               // 0b: Program manual
                Assert.AreEqual(32767, data[16]);           // 10: ISO 16383
                Assert.AreEqual(70, data[23]);              // 17: Lens 24-70
            }
        }

        [TestMethod]
        public void DumpShootInfo()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // 3)  0x0004 UShort 16-bit: [0x00000654] (34): 68, 0, 160, 65324, 108, 65376, 0, 0, 3, 0, 8, 8, 148, 0, 0, 0, 0, 0, 1, 0, 0, 108, 65376, 45, 0, 0, 248, 65535, 65535, 65535, 65535, 0, 0, 0, 
                var data = RawImage.ReadUInts16(binaryReader, notes.Entries.Single(e => e.TagId == 0x0004 && e.TagType == 3));

                Assert.AreEqual(2 * data.Length, data[0]);
                Assert.AreEqual(160, data[2]);
                Assert.AreEqual(0, data[9]);
                Assert.AreEqual(148, data[12]);
                //1   AutoISO int16s  (actual ISO used = BaseISO * AutoISO / 100)
                //2   BaseISO int16s
                //3   MeasuredEV int16s  (this is the Canon name for what could better be called MeasuredLV, and should be close to the calculated LightValue for a proper exposure with most models)
                //4   TargetAperture int16s
                //5   TargetExposureTime int16s
                //6   ExposureCompensation int16s
                //7   WhiteBalance int16s  --> Canon WhiteBalance Values
                //8   SlowShutter int16s  -1 = n / a
                //                         0 = Off
                //                         1 = Night Scene
                //                         2 = On
                //                         3 = None
                //9   SequenceNumber int16s  (valid only for some models)
                //10  OpticalZoomCode int16s  (for many PowerShot models, a this is 0 - 6 for wide - tele zoom)
                //12  CameraTemperature int16s  (newer EOS models only)
                //13  FlashGuideNumber int16s
                //14  AFPointsInFocus int16s  (used by D30, D60 and some PowerShot / Ixus models)
                //                         0x3000 = None(MF)
                //                         0x3001 = Right
                //                         0x3002 = Center
                //                         0x3003 = Center + Right       
                //                         0x3004 = Left
                //                         0x3005 = Left + Right
                //                         0x3006 = Left + Center
                //                         0x3007 = All
                //15  FlashExposureComp int16s
                //16  AutoExposureBracketing int16s  -1 = On
                //                         0 = Off
                //                         1 = On(shot 1)
                //                         2 = On(shot 2)
                //                         3 = On(shot 3)
                //17  AEBBracketValue int16s
                //18  ControlMode int16s  0 = n / a
                //                         1 = Camera Local Control
                //                         3 = Computer Remote Control
                //19  FocusDistanceUpper int16u  (FocusDistance tags are only extracted if FocusDistanceUpper is non - zero)
                //20  FocusDistanceLower int16u
                //21  FNumber int16s
                //22  ExposureTime int16s
                //23  MeasuredEV2 int16s
                //24  BulbDuration int16s
                //26  CameraType int16s  0 = n / a
                //                         248 = EOS High - end
                //                         250 = Compact
                //                         252 = EOS Mid - range
                //                         255 = DV Camera
                //27  AutoRotate int16s  -1 = n / a
                //                         0 = None
                //                         1 = Rotate 90 CW
                //                         2 = Rotate 180
                //                         3 = Rotate 270 CW
                //28  NDFilter int16s  -1 = n / a
                //                         0 = Off
                //                         1 = On
                //29  SelfTimer2 int16s
                //33  FlashOutput int16s  (used only for PowerShot models, this has a maximum value of 500 for models like the A570IS)
            }
        }

        [TestMethod]
        public void DumpCameraInfo()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // 7)  0x000D UByte[]: [0x000006F0] (1536):                     // camera info
                var entry = notes.Entries.Single(e => e.TagId == 0x000D && e.TagType == 7);
                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                var data = RawImage.ReadChars(binaryReader, entry);

                //var xxx = new ImageFileDirectory(binaryReader);
                // "ªª\u0010#\u0010#H\0\0\u0087\0i\0\u0003\0\0\0\0\0\0\u0001\0\0\u0006\0\0\0\u0094\u0002\0Z\0H\0Z\0 \0\0\0\0\0\0\0\0\u0003\0\0\0\0\0\0\0 \0\0\u0001»»HP\0\u001a\u0001\0ñ\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0002\0\0\0ÿ\0H\0\"\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0001\u0001\0\0\0\0\u0003\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0004§\u0003\r\0\0\0\0\0\0\0\0\0ÿÿÿÿ\fÌÌ\u0002\0\0\0\u0001\0\0\0\0\0\0\0H\0\0\0\0\0\0\0\0H\0\0\0\0j\0\0\0\0\0P\u0014\0\0\0\0\0\0\0\0\0\0\u0006\0\0\0\u0006\0\0\0\u0006\0\0\0\u0004\0\0\0\u0004\0\0\0\u0004\0\0\0\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0085\0\0\0\u0001\0\0\0\u0001\0\0\0\u0001\0\0\0\a\0\u0001\0\0\0\0\u0003\u0005\u0003ÿÿ\u0003\a\u0001\0\0\0\0\0\a\0\0\0\0\0\0\u0016\u0001\0\0\0\u0001\0\u0001\u0003\0\0\0\0\0\0\0\0\0\0\0\0\0\t\u0001\0\0\0\0\0\0\0\u0006\u0001\0\0\a\u0003\u0003\u0003\u0002ÿ\0\0\0\0\0\0\0\0\u0001 P\0æ\0\u0018\0F\u0091g\u0002\0\0ÿ\0\0\0\0\0\0\0\0\0\0\0\0\0\0!\0\0\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0001\0\0\0\u0080\u0016\0\0\0\u000f\0\0à\u0010\0\0\u0080\n\0\0\u0080\u0004\0\0\0\u0003\0\0Ð\u0002\0\0à\u0001\0\0\0\0\0\0\0\0\0\0Ð\u0002\0\0à\u0001\0\0Ð\u0002\0\0à\u0001\0\0\0\0\0\0\0\0\0\0Ð\u0002\0\0à\u0001\0\0\0\0\0\0\0\0\0\0\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿ\0\u0002\0\0\0\0\0\0\0\0\0\0\n\u0002\0\u0001\0\0\u0001\0\0\u0002\u0001\0\0\0\0\0\0\0\u0001\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\01.2.3\0A6(34)\0VQ\0\u0018\u0002\u0098\u0019Xþ\u0013\0dî\0\0dî\0\0\0\0\0\0Greg Eakin\0\0\0a3a\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0e\0\0\0d\0\0\0d\0\0\0\u0082\0\0\0Ñ \0\0\0\0\0\0f\0\0\0e\0\0\0d\0\0\0\b\0\0\0\b\0\0\0\b\0\0\0\b\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0î\u0001\0\u0004\0\u0004Ú\u0002\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ë\u0001\0\u0004\0\u0004~\u0002\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ë\u0001\0\u0004\0\u0004~\u0002\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ë\u0001\0\u0004\0\u0004~\u0002\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ë\u0001\0\u0004\0\u0004~\u0002\0\0\0\0\0\0\0\0\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0001\0\0\0\0\0\0\0\u0003\0\0\0\0\0\0\0\0\0\0\0ï¾­Þï¾­Þ\0\0\0\0\u0002\0\0\0\0\0\0\0\0\0\0\0ï¾­Þï¾­Þ\0\0\0\0\u0004\0\0\0\0\0\0\0\0\0\0\0ï¾­Þï¾­Þ\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ï¾­Þï¾­Þ\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ï¾­Þï¾­Þ\0\0\0\0\u0003\0\0\0ï¾­Þï¾­Þ\0\0\0\0\0\0\0\0\0\0\0\0\u0003\0\0\0\0\0\0\0\0\0\0\0ï¾­Þï¾­Þ\0\0\0\0\u0003\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0003\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0003\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0081\0\u0081\0\u0081\0\0\0ÿÿÿÿÿÿÿÿ\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0004\0\u0004\0\u0004\0\u0004\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0004\0\u0004\0\u0004\0\u0004\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0004\0\u0004\0\u0004\0\u0004\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0004\0\u0004\0\u0004\0\u0004\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\u0004\0\u0004\0\u0004\0\u0004\0\0\0\0\0\0\0\0q\b9S\t\0\0\0\0\0\u0001\0\0\0\u0001\0\0\0\0\0\0\0\0\0\0\0\u0001\0\0\0\0\0\0\0\0#\0\0\0\0\0\0\0\0\0\0\0\0À6\0\0Ä9\u0001\0\0\0\0\0\0\0\0\0\0\0\0\0\u0003\0\0\0S\0\0\0X\0\u0003\u0080\0\0\0\0\u0003\0\0\0M\0\0\0N\0\0\0\0\0\0\0\u0003\0\0\03\0\0\0;\0\u0003\u0080\0\0\0\0\u0003\0\0\0T"
            }
        }

        [TestMethod]
        public void DumpSensorInfo1()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // Sensor Info
                // 24)  0x00E0 UShort 16-bit: [0x00001540] (17): 34, 5920, 3950, 1, 1, 140, 96, 5899, 3935, 0, 0, 0, 0, 0, 0, 0, 0, 
                var data = RawImage.ReadUInts16(binaryReader, notes.Entries.Single(e => e.TagId == 0x00E0 && e.TagType == 3));
                Assert.AreEqual(2 * data.Length, data[0]);
                //Assert.AreEqual(5920, data[1]);   // sensor width
                //Assert.AreEqual(3950, data[2]);   // sensor height
                //Assert.AreEqual(1, data[3]);      // left
                //Assert.AreEqual(1, data[4]);      // top
                //Assert.AreEqual(140, data[5]);      // mask left
                //Assert.AreEqual(96, data[6]);      // mask top
                //Assert.AreEqual(5899, data[7]);      // mask right
                //Assert.AreEqual(3935, data[8]);      // mask bottom

                var imageWidth = (int)image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(imageWidth, data[7] - data[5] + data[3]);

                var imageLength = (int)image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(imageLength, data[8] - data[6] + data[4]);
            }
        }

        [TestMethod]
        public void DumpMakerNotes2()
        {
            const string fileName = @"C:..\..\Photos\7DSraw.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image[0x8769];
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif[0x927c];
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                Assert.AreEqual("Canon EOS 7D", model);

                var firmware = RawImage.ReadChars(binaryReader, notes[0x0007]);
                Assert.AreEqual("Firmware Version 2.0.5\0", firmware);

                // 0x0010 ULong 32 - bit: 2147484240
                var id = notes[0x0010];
                Assert.AreEqual(0x80000250, id.ValuePointer);
            }
        }

        [TestMethod]
        public void DumpSensorInfo2()
        {
            const string fileName = @"C:..\..\Photos\7DSraw.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // Sensor Info
                var data = RawImage.ReadUInts16(binaryReader, notes.Entries.Single(e => e.TagId == 0x00E0 && e.TagType == 3));
                Assert.AreEqual(2 * data.Length, data[0]);

                var imageWidth = (int)image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(imageWidth / 2, data[7] - data[5] + data[3]);

                var imageLength = (int)image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(imageLength / 2, data[8] - data[6] + data[4]);
            }
        }


        [TestMethod]
        public void DumpMakerNotes3()
        {
            const string fileName = @"C:..\..\Photos\7Dhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image[0x8769];
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif[0x927c];
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);
                Assert.AreEqual(41, notes.Entries.Length);

                var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                Assert.AreEqual("Canon EOS 7D", model);

                var firmware = RawImage.ReadChars(binaryReader, notes[0x0007]);
                Assert.AreEqual("Firmware Version 2.0.3\0", firmware);

                // 0x0010 ULong 32 - bit: 2147484240
                var id = notes[0x0010];
                Assert.AreEqual(0x80000250, id.ValuePointer);

                // notes.DumpDirectory(binaryReader);
            }
        }

        [TestMethod]
        public void DumpSensorInfo3()
        {
            const string fileName = @"C:..\..\Photos\7Dhigh.CR2";
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var exifEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                binaryReader.BaseStream.Seek(exifEntry.ValuePointer, SeekOrigin.Begin);
                var exif = new ImageFileDirectory(binaryReader);

                var notesEntry = exif.Entries.Single(e => e.TagId == 0x927C && e.TagType == 7);
                binaryReader.BaseStream.Seek(notesEntry.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                // Sensor Info
                var data = RawImage.ReadUInts16(binaryReader, notes.Entries.Single(e => e.TagId == 0x00E0 && e.TagType == 3));
                Assert.AreEqual(2 * data.Length, data[0]);

                var imageWidth = (int)image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(imageWidth, data[7] - data[5] + data[3]);

                var imageLength = (int)image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(imageLength, data[8] - data[6] + data[4]);
            }
        }
    }
}
