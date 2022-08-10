// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		HuffmanTableTests.cs
// AUTHOR:		Greg Eakin

using System.Linq;
using PhotoLib.Jpeg;
using Xunit;

namespace PhotoTests.Jpeg;

public class HuffmanTableTests
{
    [Fact]
    public void BuildTreeDataTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        Assert.Equal(16, data1.Length);
        Assert.Equal(data2.Length, data1.Sum(b => b));
        Assert.True(data2.Length <= 256);
    }

    [Fact]
    public void HuffmanTableTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var huffmanTable = new HuffmanTable(0, data1, data2);
        Assert.Equal(0, huffmanTable.Index);
        Assert.Same(data1, huffmanTable.Data1);
        Assert.Same(data2, huffmanTable.Data2);
        Assert.Equal(15, huffmanTable.Dictionary.Count);
    }

    [Fact]
    public void PrintBits1()
    {
        var z = HuffmanTable.PrintBits(0x01, 0x00);
        Assert.Equal("1", z);
    }

    [Fact]
    public void PrintBits2()
    {
        var z = HuffmanTable.PrintBits(0x02, 0x02);
        Assert.Equal("010", z);
    }

    [Fact]
    public void DcCodeTestFour()
    {
        for (var i = 8; i < 16; i++)
        {
            var expected = i;
            Assert.Equal(expected, HuffmanTable.DcValueEncoding(4, (byte)i));
        }
    }

    [Fact]
    public void DcCodeTestFourNegative()
    {
        for (var i = 0; i < 8; i++)
        {
            var expected = i - 15;
            Assert.Equal(expected, HuffmanTable.DcValueEncoding(4, (byte)i));
        }
    }

    [Fact]
    public void DcCodeTestOne()
    {
        Assert.Equal(1, HuffmanTable.DcValueEncoding(1, 1));
        Assert.Equal(-1, HuffmanTable.DcValueEncoding(1, 0));
    }

    [Fact]
    public void DcCodeTestSimple()
    {
        // 0 1111 1111 1
        Assert.Equal(-512, HuffmanTable.DcValueEncoding(10, (ushort)0x01FFu));
    }

    [Fact]
    public void DcCodeTestZero()
    {
        Assert.Equal(0, HuffmanTable.DcValueEncoding(0, 0));
    }

    [Fact]
    public void BuildTreeKeysTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var keys = new[] { 0, 1, 4, 5, 12, 13, 28, 29, 30, 62, 126, 254, 510, 1022, 2046 };

        var dictionary = HuffmanTable.BuildTree(data1, data2);

        Assert.Equal(keys, dictionary.Keys);
    }

    [Fact]
    public void BuildTreeCodesTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var codes = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var dictionary = HuffmanTable.BuildTree(data1, data2);

        Assert.Equal(codes, dictionary.Values.Select(key => key.Code).ToArray());
    }

    [Fact]
    public void BuildTreeLengthTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var lengths = new byte[] { 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 7, 8, 9, 10, 11 };

        var dictionary = HuffmanTable.BuildTree(data1, data2);

        Assert.Equal(lengths, dictionary.Values.Select(key => key.Length).ToArray());
    }

    [Fact]
    public void TextTreeTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var treeBits = new[]
        {
            "00", "01",
            "100", "101",
            "1100", "1101",
            "11100", "11101", "11110",
            "111110",
            "1111110",
            "11111110",
            "111111110",
            "1111111110",
            "11111111110"
        };

        Assert.Equal(treeBits, HuffmanTable.ToTextTree(data1, data2));
    }

    [Fact]
    public void DecodeDifBitsTest()
    {
        Assert.Equal(0, HuffmanTable.DecodeDifBits(0, 0));

        Assert.Equal(-1, HuffmanTable.DecodeDifBits(1, 0));
        Assert.Equal(1, HuffmanTable.DecodeDifBits(1, 1));

        Assert.Equal(-3, HuffmanTable.DecodeDifBits(2, 0));
        Assert.Equal(-2, HuffmanTable.DecodeDifBits(2, 1));
        Assert.Equal(2, HuffmanTable.DecodeDifBits(2, 2));
        Assert.Equal(3, HuffmanTable.DecodeDifBits(2, 3));

        Assert.Equal(-7, HuffmanTable.DecodeDifBits(3, 0));
        Assert.Equal(-6, HuffmanTable.DecodeDifBits(3, 1));
        Assert.Equal(-5, HuffmanTable.DecodeDifBits(3, 2));
        Assert.Equal(-4, HuffmanTable.DecodeDifBits(3, 3));
        Assert.Equal(4, HuffmanTable.DecodeDifBits(3, 4));
        Assert.Equal(5, HuffmanTable.DecodeDifBits(3, 5));
        Assert.Equal(6, HuffmanTable.DecodeDifBits(3, 6));
        Assert.Equal(7, HuffmanTable.DecodeDifBits(3, 7));

        for (var bits = 4; bits < 16; bits++)
        {
            var x = 0x01u << bits;
            var y = 0x01u << (bits - 1);

            for (var codes = 0u; codes < (0x01u << bits); codes++)
            {
                if (codes < y)
                {
                    var expected = -x + 1 + codes;
                    Assert.Equal((short)expected, HuffmanTable.DecodeDifBits((ushort)bits, (ushort)codes));
                }
                else
                {
                    var expected = codes;
                    Assert.Equal((short)expected, HuffmanTable.DecodeDifBits((ushort)bits, (ushort)codes));
                }
            }
        }

        // Assert.Equal(32768, HuffmanTable.DecodeDifBits(16, 0));
    }

    [Fact]
    public void ToStringTest()
    {
        var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

        var huffmanTable = new HuffmanTable(0, data1, data2);
        Assert.Equal("HuffmanTable DC 0\r\n" + 
                     " 2 : 00 (00) 05 (01) \r\n" + 
                     " 3 : 03 (100) 06 (101) \r\n" + 
                     " 4 : 07 (1100) 02 (1101) \r\n" + 
                     " 5 : 08 (11100) 00 (11101) 01 (11110) \r\n" + 
                     " 6 : 09 (111110) \r\n" + 
                     " 7 : 0A (1111110) \r\n" + 
                     " 8 : 0B (11111110) \r\n" + 
                     " 9 : 0C (111111110) \r\n" + 
                     "10 : 0D (1111111110) \r\n" + 
                     "11 : 0F (11111111110) \r\n", huffmanTable.ToString());
    }
}
