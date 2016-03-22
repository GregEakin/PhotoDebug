using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class GpsData
    {
        [TestMethod]
        public void DumpGpsData()
        {
            //const string fileName = @"D:\Users\Greg\Pictures\2016-03-20 GPS\0L2A2368.CR2";
            //const string fileName = @"D:\Users\Greg\Pictures\2016_03_21\0L2A2373.CR2";
            //DumpGpsInfo(fileName);

            var fileEntries = Directory.GetFiles(@"D:\Users\Greg\Pictures\2016-03-20 GPS\");
            foreach (var fileName in fileEntries.Where(file => file.EndsWith(".CR2")))
                DumpGpsInfo(fileName);
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
                // Assert.AreEqual(70028u, gps);

                DumpGpsInfo(binaryReader, gps);
            }
        }

        private static string DumpData(ImageFileEntry entry)
        {
            var bytes = new[]
            {
                (byte)(entry.ValuePointer >>  0 & 0xFF),
                (byte)(entry.ValuePointer >>  8 & 0xFF),
                (byte)(entry.ValuePointer >> 16 & 0xFF),
                (byte)(entry.ValuePointer >> 24 & 0xFF),
            };
            var str = Encoding.ASCII.GetString(bytes, 0, (int)entry.NumberOfValue - 1);
            return str;
        }

        private static void DumpGpsInfo(BinaryReader binaryReader, uint offset)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var tags = new ImageFileDirectory(binaryReader);
            Assert.AreEqual(0x00000302u, tags.Entries[0x0000].ValuePointer);    // version number?
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
            Console.WriteLine("Satellite signal status {0}", DumpData(tags.Entries[0x0009]));

            var date = RawImage.ReadChars(binaryReader, tags.Entries[0x001D]);
            var timeData = RawImage.ReadRational(binaryReader, tags.Entries[0x0007]);
            var time1 = (double)timeData[0] / timeData[1];
            var time2 = (double)timeData[2] / timeData[3];
            var time3 = (double)timeData[4] / timeData[5];
            Console.WriteLine("Date / Time {0}, {1} {2} {3} UTC", date, time1, time2, time3);

            var latitudeData = RawImage.ReadRational(binaryReader, tags.Entries[0x0002]);
            var latitude1 = (double)latitudeData[0] / latitudeData[1];
            var latitude2 = (double)latitudeData[2] / latitudeData[3];
            var latitude3 = (double)latitudeData[4] / latitudeData[5];
            var latitudeDirection = DumpData(tags.Entries[0x0001]);
            Console.WriteLine("Latitude {0} {1} {2} {3}", latitude1, latitude2, latitude3, latitudeDirection);

            var longitudeData = RawImage.ReadRational(binaryReader, tags.Entries[0x0004]);
            var longitude1 = (double)longitudeData[0] / longitudeData[1];
            var longitude2 = (double)longitudeData[2] / longitudeData[3];
            var longitude3 = (double)longitudeData[4] / longitudeData[5];
            var longitudeDirection = DumpData(tags.Entries[0x0003]);
            Console.WriteLine("Longitude {0} {1} {2} {3}", longitude1, longitude2, longitude3, longitudeDirection);

            var altitudeData = RawImage.ReadRational(binaryReader, tags.Entries[0x0006]);
            var altitude = (double)altitudeData[0] / altitudeData[1];
            Assert.AreEqual("M", DumpData(tags.Entries[0x0010]));                       // units ?
            Console.WriteLine("Altitude {0:0.00} m", altitude);

            Console.WriteLine("Geographic coordinate system {0}", RawImage.ReadChars(binaryReader, tags.Entries[0x0012]));

            var directionData = RawImage.ReadRational(binaryReader, tags.Entries[0x0011]);
            var direction = (double)directionData[0] / directionData[1];
            Console.WriteLine("direction {0}", direction);

            var dopData = RawImage.ReadRational(binaryReader, tags.Entries[0x000B]);
            var dop = (double)dopData[0] / dopData[1];
            Console.WriteLine("Dilution of Position {0}", dop);

            //Fix quality: 0 = invalid
            //             1 = GPS fix(SPS)
            //             2 = DGPS fix
            //             3 = PPS fix
            //             4 = Real Time Kinematic
            //             5 = Float RTK
            //             6 = estimated(dead reckoning)(2.3 feature)
            //             7 = Manual input mode
            //             8 = Simulation mode
            Console.WriteLine("Fix quality = {0}", DumpData(tags.Entries[0x000A]));
            Console.WriteLine("Number of satellites = {0}", DumpData(tags.Entries[0x0008]));

            // lots of zero data
            Assert.AreEqual(0x00000000u, tags.Entries[0x0005].ValuePointer);
            Assert.AreEqual("", DumpData(tags.Entries[0x000C]));
            var data0D = RawImage.ReadRational(binaryReader, tags.Entries[0x000D]);
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data0D);
            Assert.AreEqual("", DumpData(tags.Entries[0x000E]));
            var data0F = RawImage.ReadRational(binaryReader, tags.Entries[0x000F]);
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data0F);
            Assert.AreEqual("", DumpData(tags.Entries[0x0013]));
            var data14 = RawImage.ReadRational(binaryReader, tags.Entries[0x0014]);
            CollectionAssert.AreEqual(new uint[] { 0, 1, 0, 1, 0, 1 }, data14);
            Assert.AreEqual("", DumpData(tags.Entries[0x0015]));
            var data16 = RawImage.ReadRational(binaryReader, tags.Entries[0x0016]);
            CollectionAssert.AreEqual(new uint[] { 0, 1, 0, 1, 0, 1 }, data16);
            Assert.AreEqual("", DumpData(tags.Entries[0x0017]));
            var data18 = RawImage.ReadRational(binaryReader, tags.Entries[0x0018]);
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data18);
            Assert.AreEqual("", DumpData(tags.Entries[0x0019]));
            var data1A = RawImage.ReadRational(binaryReader, tags.Entries[0x001A]);
            CollectionAssert.AreEqual(new uint[] { 0, 1 }, data1A);
            var data1B = RawImage.ReadBytes(binaryReader, tags.Entries[0x001B]);
            Assert.AreEqual(256, data1B.Length);
            foreach (var b in data1B)
                Assert.AreEqual(0x00, b);
            var data1C = RawImage.ReadBytes(binaryReader, tags.Entries[0x001C]);
            foreach (var b in data1C)
                Assert.AreEqual(0x00, b);
            Assert.AreEqual(256, data1C.Length);
            Assert.AreEqual(0x0000u, tags.Entries[0x001E].ValuePointer);

            // Model GP-E2
            // firmware 2.0.0
            // serial number 3410400095
        }
    }
}
