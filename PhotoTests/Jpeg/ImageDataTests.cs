// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		ImageDataTests.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using PhotoLib.Jpeg;
using Xunit;

namespace PhotoTests.Jpeg;

public class ImageDataTests
{
    [Fact]
    public void IndexBegin()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(0, imageData.Index);
    }

    [Fact]
    public void Index0()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.True(imageData.GetNextBit());
        Assert.Equal(0, imageData.Index);
    }

    [Fact]
    public void Index1()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(165, imageData.GetSetOfBits(8));
        Assert.Equal(1, imageData.Index);
    }

    [Fact]
    public void Index2()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(165, imageData.GetSetOfBits(8));
        Assert.Equal(90, imageData.GetSetOfBits(8));
        Assert.Equal(2, imageData.Index);
    }

    [Fact]
    public void EndOfFileBegin()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.False(imageData.EndOfFile);
    }

    [Fact]
    public void EndOfFile0()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.True(imageData.GetNextBit());
        Assert.False(imageData.EndOfFile);
    }

    [Fact]
    public void EndOfFile1()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(165, imageData.GetSetOfBits(8));
        Assert.False(imageData.EndOfFile);
    }

    [Fact]
    public void EndOfFile2()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(165, imageData.GetSetOfBits(8));
        Assert.Equal(90, imageData.GetSetOfBits(8));
        Assert.True(imageData.EndOfFile);
    }

    [Fact]
    public void DistFromEnd0()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.True(imageData.GetNextBit());
        Assert.Equal(2, imageData.DistFromEnd);
    }

    [Fact]
    public void DistFromEnd1()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(165, imageData.GetSetOfBits(8));
        Assert.Equal(1, imageData.DistFromEnd);
    }

    [Fact]
    public void DistFromEnd2()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(165, imageData.GetSetOfBits(8));
        Assert.Equal(90, imageData.GetSetOfBits(8));
        Assert.Equal(0, imageData.DistFromEnd);
    }

    [Fact]
    public void RawData()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(data, imageData.RawData);
    }

    [Fact]
    public void GetBit()
    {
        var data = new byte[] { 0xA5 };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.True(imageData.GetNextBit());
        Assert.False(imageData.GetNextBit());
        Assert.True(imageData.GetNextBit());
        Assert.False(imageData.GetNextBit());
        Assert.False(imageData.GetNextBit());
        Assert.True(imageData.GetNextBit());
        Assert.False(imageData.GetNextBit());
        Assert.True(imageData.GetNextBit());
    }

    [Fact]
    public void GetBits2()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.True(imageData.GetNextBit());
        Assert.False(imageData.GetNextBit());
        Assert.Equal(0x0009, imageData.GetSetOfBits(4));
        Assert.Equal(0x0001, imageData.GetSetOfBits(2));
        Assert.Equal(0x005A, imageData.GetSetOfBits(8));
    }

    [Fact]
    public void GetBits3()
    {
        var data = new byte[] { 0xA5, 0x5A };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        Assert.Equal(0x000A, imageData.GetSetOfBits(4));
        Assert.Equal(0x0055, imageData.GetSetOfBits(8));
        Assert.Equal(0x000A, imageData.GetSetOfBits(4));
    }

    [Fact]
    public void GetValuesTest()
    {
        var table0 = new HuffmanTable(0,
            new byte[] { 0x00, 0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });
        Console.WriteLine("Table 0 {0}", table0);

        var table1 = new HuffmanTable(1,
            new byte[] { 0x00, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00 },
            new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });
        Console.WriteLine("Table 1 {0}", table1);

        var data = new byte[] { 0xff, 0x00, 0xe0, 0x0b, 0xa2, 0x89, 0x68, 0xc7, 0x00, 0xb0 };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var imageData = new ImageData(reader, (uint)data.Length);
        var prevY = 0x4000u;
        var prevCb = 0x0000;
        var prevCr = 0x0000;

        Assert.Equal(14, imageData.GetValue(table0));
        Assert.Equal(0x2E, imageData.GetValue(14));
        Assert.Equal(-16337, HuffmanTable.DecodeDifBits(14, 0x2E));
        prevY -= 16337;
        Assert.Equal(47u, prevY);

        Assert.Equal(3, imageData.GetValue(table0));
        Assert.Equal(0x2, imageData.GetValue(3));
        Assert.Equal(-5, HuffmanTable.DecodeDifBits(3, 0x2));
        prevY -= 5;
        Assert.Equal(42u, prevY);

        Assert.Equal(2, imageData.GetValue(table1));
        Assert.Equal(0x0, imageData.GetValue(2));
        Assert.Equal(-3, HuffmanTable.DecodeDifBits(2, 0x0));
        prevCb += -3;
        // Assert.Equal(16381, prevCb);
        Assert.Equal(-3, prevCb);

        Assert.Equal(2, imageData.GetValue(table1));
        Assert.Equal(0x1, imageData.GetValue(2));
        Assert.Equal(-2, HuffmanTable.DecodeDifBits(2, 0x1));
        prevCr += -2;
        //Assert.Equal(16382, prevCr);
        Assert.Equal(-2, prevCr);

        Assert.Equal(2, imageData.GetValue(table0));
        Assert.Equal(0x1, imageData.GetValue(2));
        Assert.Equal(-2, HuffmanTable.DecodeDifBits(2, 0x1));
        prevY -= 2;
        Assert.Equal(40u, prevY);

        Assert.Equal(0, imageData.GetValue(table0));
        Assert.Equal(0x0, imageData.GetValue(0));
        Assert.Equal(0, HuffmanTable.DecodeDifBits(0, 0));
        prevY += 0;
        Assert.Equal(40u, prevY);

        Assert.Equal(1, imageData.GetValue(table1));
        Assert.Equal(0x1, imageData.GetValue(1));
        Assert.Equal(1, HuffmanTable.DecodeDifBits(1, 0x1));
        prevCb += 1;
        //Assert.Equal(16382, prevCb);
        Assert.Equal(-2, prevCb);

        Assert.Equal(0, imageData.GetValue(table1));
        Assert.Equal(0x0, imageData.GetValue(0));
        Assert.Equal(0, HuffmanTable.DecodeDifBits(0, 0x0));
        prevCr += 0;
        // Assert.Equal(16382, prevCr);
        Assert.Equal(-2, prevCr);

        Assert.Equal(2, imageData.GetValue(table0));
        Assert.Equal(0x2, imageData.GetValue(2));
        Assert.Equal(2, HuffmanTable.DecodeDifBits(2, 0x2));
        prevY += 2;
        Assert.Equal(42u, prevY);

        Assert.Equal(0, imageData.GetValue(table0));
        Assert.Equal(0x0, imageData.GetValue(0));
        Assert.Equal(0, HuffmanTable.DecodeDifBits(0, 0));
        prevY += 0;
        Assert.Equal(42u, prevY);

        Assert.Equal(0, imageData.GetValue(table1));
        Assert.Equal(0x0, imageData.GetValue(0));
        Assert.Equal(0, HuffmanTable.DecodeDifBits(0, 0x0));
        prevCb += 0;
        // Assert.Equal(16382, prevCb);
        Assert.Equal(-2, prevCb);

        Assert.Equal(0, imageData.GetValue(table1));
        Assert.Equal(0x0, imageData.GetValue(0));
        Assert.Equal(0, HuffmanTable.DecodeDifBits(0, 0x0));
        prevCr += 0;
        // Assert.Equal(16382, prevCr);
        Assert.Equal(-2, prevCr);
    }
}
