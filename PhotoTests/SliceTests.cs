using System.IO;
using Xunit;

namespace PhotoTests;

public class SliceTests
{
    private const string FileName = @"\\Data\Photo\2018\2018-08-29\0L2A3743.CR2";

    [Fact]
    public void RawImageDumpData()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        // using var binaryReader = new BinaryReader(fileStream);

    }
}
