// Log File Viewer - CameraInfoTests.cs
// 
// Copyright © 2018 Greg Eakin. 
// 
// Greg Eakin <greg@gdbtech.info>
// 
// All Rights Reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class CameraInfoTests
    {
        [TestMethod]
        public void DumpCameraInfoTest()
        {
            foreach (var dir in Directory.EnumerateDirectories(@"D:\Users\Greg\Pictures"))
            {
                if (!dir.EndsWith("EOS R")) continue;

                foreach (var fileName in Directory.EnumerateFiles(dir))
                {
                    if (!fileName.EndsWith(".CR2") && !fileName.EndsWith(".CR3")) continue;
                    DumpIndex(fileName);
                }
            }
        }

        // 
        [TestMethod]
        public void FigureInfoTest()
        {
            var fileName = @"D:\Users\Greg\Pictures\2017-01-26 Party\IMG_0001.CR2";
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

                Console.Write("{0}: ", fileName);

                var id = notes[0x0010];
                Console.WriteLine(id.ValuePointer);

                //var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
                //Console.Write("{0}, ", model);

                //var serial = RawImage.ReadChars(binaryReader, notes[0x000c]);
                //Console.Write("{0}, ", serial);

                //Console.WriteLine();

                // notes.DumpDirectory(binaryReader);
            }
        }

        public static void DumpIndex(string fileName)
        {
            // const string fileName = @"D:\Users\Greg\Pictures\2018-10-11\0L2A4224.CR2";
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

                var modelId = notes[0x0010].ValuePointer;
                switch (modelId)
                {
                    case 0x0374000u:
                        // EOS M3
                        // Other(fileName, notes, binaryReader);
                        break;
                    case 0x0394000u:
                        // EOS M5
                        // Other(fileName, notes, binaryReader);
                        break;
                    case 0x80000250u:
                        // Canon EOS 7D
                        Canon7D(fileName, notes, binaryReader);
                        break;
                    case 0x80000285u:
                        // Canon EOS 5D Mark III
                        // Canon5D3(fileName, notes, binaryReader);
                        break;
                    case 0x80000289u:
                        // Canon EOS 7D Mark II
                        // Canon7D2(fileName, notes, binaryReader);
                        break;
                    case 0x80000424:
                        // EOS R
                        Console.WriteLine("EOS R");
                        break;
                    default:
                        Other(fileName, notes, binaryReader);
                        break;
                }
            }
        }

        private static void Other(string fileName, ImageFileDirectory notes, BinaryReader binaryReader)
        {
            Console.Write("{0}: ", fileName);
            var id = notes[0x0010].ValuePointer;
            Console.Write("{0}, ", id);
            var model = RawImage.ReadChars(binaryReader, notes[0x0006]);
            Console.Write("{0}", model);
            //notes.DumpDirectory(binaryReader);

            Console.WriteLine();
        }

        private static void Canon7D(string fileName, ImageFileDirectory notes, BinaryReader binaryReader)
        {
            var infoEntry = notes.Entries.Single(e => e.TagId == 0x000D && e.TagType == 7);
            binaryReader.BaseStream.Seek(infoEntry.ValuePointer, SeekOrigin.Begin);
            Console.Write("{0}: ", fileName);
            var info = binaryReader.ReadBytes((int) infoEntry.NumberOfValue);

            var index1 = BitConverter.ToInt32(info, 491);
            Console.WriteLine("{0}", index1);
        }

        private static void Canon7D2(string fileName, ImageFileDirectory notes, BinaryReader binaryReader)
        {
            return;

            var infoEntry = notes.Entries.Single(e => e.TagId == 0x000D && e.TagType == 7);
            binaryReader.BaseStream.Seek(infoEntry.ValuePointer, SeekOrigin.Begin);
            Console.Write("{0}: ", fileName);
            var info = binaryReader.ReadBytes((int) infoEntry.NumberOfValue);

            var index1 = BitConverter.ToInt32(info, 491);
            Console.WriteLine("{0}", index1);
        }

        private static void Canon5D3(string fileName, ImageFileDirectory notes, BinaryReader binaryReader)
        {
            var infoEntry = notes.Entries.Single(e => e.TagId == 0x000D && e.TagType == 7);
            binaryReader.BaseStream.Seek(infoEntry.ValuePointer, SeekOrigin.Begin);
            Console.Write("{0}: ", fileName);
            var info = binaryReader.ReadBytes((int) infoEntry.NumberOfValue);

            var index1 = BitConverter.ToInt32(info, 0x0292);
            var index2 = BitConverter.ToInt32(info, 0x0296);
            Console.Write("{0}, {1}: ", index1, index2);

            var index3 = BitConverter.ToInt32(info, 0x029E);
            var index4 = BitConverter.ToInt32(info, 0x02A2);
            Console.Write("{0}, {1} ", index3, index4);

            //for (var i = -20; i < 80; i+=4)
            //{
            //    var index = BitConverter.ToInt32(info, 0x029a + i);
            //    Console.Write("{0}, ", index);
            //}

            Console.WriteLine();
        }
    }
}