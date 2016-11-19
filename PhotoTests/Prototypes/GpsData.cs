using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class GpsData
    {
        [TestMethod]
        public void DumpGpsData()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-03-20 GPS\0L2A2368.CR2";
            //const string fileName = @"D:\Users\Greg\Pictures\2016_03_21\0L2A2373.CR2";
            //const string fileName = @"D:\Users\Greg\Pictures\2016-05-20\IMG_0008.CR2";
            DumpGpsInfo(fileName);


            //var fileEntries = Directory.GetFiles(@"D:\Users\Greg\Pictures\2016-03-28");
            //foreach (var fileName in fileEntries.Where(file => file.EndsWith(".CR2")))
            //    DumpGpsInfo(fileName);
        }

        private static void DumpGpsInfo(string fileName)
        {
            Console.WriteLine("=========");
            Console.WriteLine(fileName);

            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();
                var gps = image.Entries.Single(e => e.TagId == 0x8825 && e.TagType == 4).ValuePointer;
                DumpGpsInfo(binaryReader, gps);
            }
        }

        private static void DumpGpsInfo(BinaryReader binaryReader, uint offset)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var tags = new ImageFileDirectory(binaryReader);
            Assert.AreEqual(0x00000302u, tags.Entries.Single(e => e.TagId == 0x0000 && e.TagType == 1).ValuePointer);    // version number
            // tags.DumpDirectory(binaryReader);

            if (tags.Entries.Length == 1)
            {
                Console.WriteLine("GPS info not found....");
                return;
            }

            Assert.AreEqual(31, tags.Entries.Length);
            var expected = new List<ushort>();
            for (ushort i = 0; i < 0x001F; i++)
                expected.Add(i);
            CollectionAssert.AreEqual(expected.ToArray(), tags.Entries.Select(e => e.TagId).ToArray());

            // "A" active, "V" void
            Console.WriteLine("Satellite signal status {0}", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0009 && e.TagType == 2)));

            var date = RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x001D && e.TagType == 2));
            var timeData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0007 && e.TagType == 5));
            Assert.AreEqual(6, timeData.Length);
            var time1 = (double)timeData[0] / timeData[1];
            var time2 = (double)timeData[2] / timeData[3];
            var time3 = (double)timeData[4] / timeData[5];
            var dateTime = GpsData2.ConvertDateTime(date, time1, time2, time3);
            Console.WriteLine("Timestamp {0:M\'/\'d\'/\'yyyy\' \'h\':\'mm\':\'ss\' \'tt}", dateTime.ToLocalTime());

            var latitudeData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0002 && e.TagType == 5));
            Assert.AreEqual(6, latitudeData.Length);
            var latitude1 = (double)latitudeData[0] / latitudeData[1];
            var latitude2 = (double)latitudeData[2] / latitudeData[3];
            var latitude3 = (double)latitudeData[4] / latitudeData[5];
            var latitudeDirection = RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0001 && e.TagType == 2));
            Console.WriteLine("Latitude {0}° {1}\' {2}\" {3}", latitude1, latitude2, latitude3, latitudeDirection);

            var longitudeData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0004 && e.TagType == 5));
            Assert.AreEqual(6, longitudeData.Length);
            var longitude1 = (double)longitudeData[0] / longitudeData[1];
            var longitude2 = (double)longitudeData[2] / longitudeData[3];
            var longitude3 = (double)longitudeData[4] / longitudeData[5];
            var longitudeDirection = RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0003 && e.TagType == 2));
            Console.WriteLine("Longitude {0}° {1}\' {2}\" {3}", longitude1, longitude2, longitude3, longitudeDirection);

            var altitudeData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0006 && e.TagType == 5));
            Assert.AreEqual(2, altitudeData.Length);
            var altitude = (double)altitudeData[0] / altitudeData[1];
            Console.WriteLine("Altitude {0:0.00} m", altitude);
            Assert.AreEqual(0x00000000u, tags.Entries.Single(e => e.TagId == 0x0005 && e.TagType == 1).ValuePointer);

            Console.WriteLine("Geographic coordinate system {0}", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0012 && e.TagType == 2)));

            Assert.AreEqual("M", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0010 && e.TagType == 2)));     // Magnetic Direction
            var directionData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0011 && e.TagType == 5));
            Assert.AreEqual(2, directionData.Length);
            var direction = (double)directionData[0] / directionData[1];
            Console.WriteLine("Direction {0}°", direction);

            var dopData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x000B && e.TagType == 5));
            Assert.AreEqual(2, dopData.Length);
            var dop = (double)dopData[0] / dopData[1];
            Console.WriteLine("Dilution of Position {0}", dop);

            var quality = RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x000A && e.TagType == 2));
            Console.WriteLine("Fix quality = {0}", GpsData2.DumpFixQuality(quality));
            Console.WriteLine("Number of satellites = {0}", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0008 && e.TagType == 2)));

            // Speed
            Assert.AreEqual("", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x000C && e.TagType == 2)));
            var data0D = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x000D && e.TagType == 5));
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data0D);

            // Track
            Assert.AreEqual("", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x000E && e.TagType == 2)));
            var data0F = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x000F && e.TagType == 5));
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data0F);

            // Destination
            Assert.AreEqual("", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0013 && e.TagType == 2)));
            var data14 = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0014 && e.TagType == 5));
            CollectionAssert.AreEqual(new uint[] { 0, 1, 0, 1, 0, 1 }, data14);
            Assert.AreEqual("", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0015 && e.TagType == 2)));
            var data16 = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0016 && e.TagType == 5));
            CollectionAssert.AreEqual(new uint[] { 0, 1, 0, 1, 0, 1 }, data16);
            Assert.AreEqual("", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0017 && e.TagType == 2)));
            var data18 = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0018 && e.TagType == 5));
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data18);
            Assert.AreEqual("", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0019 && e.TagType == 2)));
            var data1A = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x001A && e.TagType == 5));
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data1A);

            // Processing Method
            var data1B = RawImage.ReadBytes(binaryReader, tags.Entries.Single(e => e.TagId == 0x001B && e.TagType == 7));
            Assert.AreEqual(256, data1B.Length);
            foreach (var b in data1B)
                Assert.AreEqual(0x00, b);

            // Area Information
            var data1C = RawImage.ReadBytes(binaryReader, tags.Entries.Single(e => e.TagId == 0x001C && e.TagType == 7));
            Assert.AreEqual(256, data1C.Length);
            foreach (var b in data1C)
                Assert.AreEqual(0x00, b);

            // Differential
            Assert.AreEqual(0x0000u, tags.Entries.Single(e => e.TagId == 0x001E && e.TagType == 3).ValuePointer);

            // Model GP-E2
            // firmware 2.0.0
            // serial number 3410400095
        }
    }
}
