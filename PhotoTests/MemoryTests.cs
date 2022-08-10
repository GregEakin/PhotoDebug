using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace PhotoTests;

public class MemoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MemoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    static void WriteInt32ToBuffer(int value, Memory<char> buffer)
    {
        var strValue = new string('a', value);

        var span = buffer.Slice(0, strValue.Length).Span;
        strValue.AsSpan().CopyTo(span);
    }

    static void DisplayBufferToConsole(ITestOutputHelper testOutputHelper, Memory<char> buffer) =>
        testOutputHelper.WriteLine($"Contents of the buffer: '{buffer}'");

    [Fact]
    public void MemTest1()
    {
        using var owner = MemoryPool<char>.Shared.Rent(100);
        try
        {
            var value = 42;

            var memory = owner.Memory;
            WriteInt32ToBuffer(value, memory);
            DisplayBufferToConsole(_testOutputHelper, memory.Slice(0, value.ToString().Length));
        }
        catch (FormatException)
        {
            _testOutputHelper.WriteLine("You did not enter a valid number.");
        }
        catch (OverflowException)
        {
            _testOutputHelper.WriteLine($"You entered a number less than {Int32.MinValue:N0} or greater than {Int32.MaxValue:N0}.");
        }
    }


    public static double ReadDouble(byte[] bytes)
    {
        var temp = bytes[0];
        bytes[0] = bytes[7];
        bytes[7] = temp;
            
        temp = bytes[1];
        bytes[1] = bytes[6];
        bytes[6] = temp;
            
        temp = bytes[2];
        bytes[2] = bytes[5];
        bytes[5] = temp;
            
        temp = bytes[3];
        bytes[3] = bytes[4];
        bytes[4] = temp;

        return BitConverter.ToDouble(bytes, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double ReadDouble(ReadOnlySpan<byte> span)
    {
        if (span.Length < 8)
            throw new ArgumentOutOfRangeException(nameof(span), "Insufficient length to decode Double from memory.");

        var num1 = (uint)((span[0] << 24) | (span[1] << 16) | (span[2] << 8) | span[3]);
        var num2 = (uint)((span[4] << 24) | (span[5] << 16) | (span[6] << 8) | span[7]);
        var val = ((ulong)num1 << 32) | num2;
        return Unsafe.As<ulong, double>(ref val);
    }
}
