using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class GpsData2
    {
        [TestMethod]
        public void DumpGpsData()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-09-04\B05A0051.CR2";
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

            Assert.AreEqual(16, tags.Entries.Length);
            var expected = new[]
            {
                (ushort)0x00, (ushort)0x01, (ushort)0x02, (ushort)0x03,
                (ushort)0x04, (ushort)0x05, (ushort)0x06, (ushort)0x07,
                (ushort)0x08, (ushort)0x09, (ushort)0x0A, (ushort)0x0B,
                (ushort)0x10, (ushort)0x11, (ushort)0x12, (ushort)0x1D
            };
            CollectionAssert.AreEqual(expected.ToArray(), tags.Entries.Select(e => e.TagId).ToArray());

            // "A" active, "V" void
            Console.WriteLine("Satellite signal status {0}", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0009 && e.TagType == 2)));

            var date = RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x001D && e.TagType == 2));
            var timeData = RawImage.ReadRational(binaryReader, tags.Entries.Single(e => e.TagId == 0x0007 && e.TagType == 5));
            Assert.AreEqual(6, timeData.Length);
            var time1 = (double)timeData[0] / timeData[1];
            var time2 = (double)timeData[2] / timeData[3];
            var time3 = (double)timeData[4] / timeData[5];
            var dateTime = ConvertDateTime(date, time1, time2, time3);
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
            Console.WriteLine("Fix quality = {0}", DumpFixQuality(quality));
            Console.WriteLine("Number of satellites = {0}", RawImage.ReadChars(binaryReader, tags.Entries.Single(e => e.TagId == 0x0008 && e.TagType == 2)));
        }

        public static DateTime ConvertDateTime(string date, double time1, double time2, double time3)
        {
            var dmy = date.Split(':');
            var year = Convert.ToInt32(dmy[0]);
            var month = Convert.ToInt32(dmy[1]);
            var day = Convert.ToInt32(dmy[2]);
            var dateTime = new DateTime(year, month, day, (int)time1, (int)time2, (int)Math.Truncate(time3), DateTimeKind.Utc);
            return dateTime.AddMilliseconds((time3 - Math.Truncate(time3)) * 1000.0);
        }

        public static string DumpFixQuality(string quality)
        {
            switch (quality)
            {
                case "0": return "invalid";
                case "1": return "GPS fix(SPS)";
                case "2": return "DGPS fix";
                case "3": return "PPS fix";
                case "4": return "Real Time Kinematic";
                case "5": return "Float RTK";
                case "6": return "estimated(dead reckoning)(2.3 feature)";
                case "7": return "Manual input mode";
                case "8": return "Simulation mode";
                default: return "unknown";
            }
        }
    }
}
