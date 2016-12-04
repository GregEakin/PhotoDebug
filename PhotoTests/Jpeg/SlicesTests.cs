// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		SlicesTests.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhotoTests.Jpeg
{
    [TestClass]
    public class SlicesTests
    {
        [TestMethod]
        public void TestBoth()
        {
            const int Height = 35;
            const int Width = 53; // = 1340 * 4
            const int X1 = 2;
            const int Y1 = 17;
            const int Z1 = 19;

            Assert.AreEqual(Width, X1 * Y1 + Z1);
            Assert.IsTrue(Y1 < Z1);

            // 5184 x 3456

            //dim = w:2592 x h:1728
            //72 dpi
            //24 bits
            //2  resultion unit

            var array1 = new int[Width * Height];
            for (var i = 0; i < Width * Height; i++)
            {
                var x = i / (Height * Y1);
                if (x < X1)
                {
                    var jrow = (i - x * Height * Y1) / Y1;
                    var y = (i - x * Height * Y1) % Y1;
                    var index = jrow * Width + x * Y1 + y;
                    array1[index] = i;
                }
                else
                {
                    var jrow = (i - X1 * Height * Y1) / Z1;
                    var z = (i - X1 * Height * Y1) % Z1;
                    var index = jrow * Width + X1 * Y1 + z;
                    array1[index] = i;
                }
            }

            var array2 = new int[Width * Height];
            var j = 0;
            for (var x = 0; x < X1; x++)
            {
                for (var jrow = 0; jrow < Height; jrow++)
                {
                    for (var y = 0; y < Y1; y++)
                    {
                        var index = jrow * Width + x * Y1 + y;
                        array2[index] = j++;
                    }
                }
            }
            for (var jrow = 0; jrow < Height; jrow++)
            {
                for (var z = 0; z < Z1; z++)
                {
                    var index = jrow * Width + X1 * Y1 + z;
                    array2[index] = j++;
                }
            }

            CollectionAssert.AreEqual(array1, array2);
        }
    }
}