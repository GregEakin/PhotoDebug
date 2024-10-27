using System.Diagnostics;

namespace ExifTool;

public class Program : IDisposable
{
    private const string ExifToolFolderPath = @"C:\Users\gregp\OneDrive\Desktop\exiftool(-k).exe";
    private const string ImageFileName = @"P:\2022\2022-07-26\MD6A1370.CR2";
    private readonly Lazy<Process> _pExifTool = new(StartProcess);

    public static void Main(string[] args)
    {
        Console.WriteLine(args.Length);
    }

    public void Dispose()
    {
        if (_pExifTool.IsValueCreated)
            _pExifTool.Value.Dispose();
    }

    private static Process StartProcess()
    {
        var toolPath = Path.Combine(@"\", ExifToolFolderPath, "exiftool.exe");
        var command = $"\"{toolPath}\" -stay_open true -@ args.txt";
        var pExifTool = new Process
        {
            Site = null,
            EnableRaisingEvents = false,
            PriorityBoostEnabled = false,
            PriorityClass = 0,
            StartInfo = new ProcessStartInfo("cmd", $"/c \"{@command}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            SynchronizingObject = null
        };

        //  NOTE:  If you do not implement an asynchronous error handler like in this example, instead simply using pExifTool.StandardError.ReadLine()
        //         in the following, you risk that your program might stall.  This is because ExifTool sometimes reports failure only through a
        //         StandardOutput line saying something like "0 output files created" without reporting anything in addition via StandardError, so
        //         pExifTool.StandardError.ReadLine() would wait indefinitely for an error message that never comes.
        pExifTool.ErrorDataReceived += ETErrorHandler;
        pExifTool.Start();
        pExifTool.BeginErrorReadLine(); //  This command starts the error handling, meaning ETErrorHandler() will now be called whenever ExifTool reports an error.

        return pExifTool;
    }

    private void RunExiftoolWhileStayingOpen()
    {
        //  Append the args file for Exiftool to start reading and executing the command.
        //  NOTE:  NEVER use WriteAllLines here - ExifTool expects the args file to be appended continually, not re-written.

        var args = new[] { ImageFileName, "-execute" }; //  This tells ExifTool to read out all of the image's metadata.
        var filename = Path.Combine(ExifToolFolderPath, "args.txt");
        File.AppendAllLines(filename, args); //  args.txt gets written into the folder where exiftool.exe resides here.

        while (true)
        {
            var line = _pExifTool.Value.StandardOutput.ReadLine();

            //  NOTE:  Depending on the command you issued, line will either contain a progress report of an operation (e.g., "1 output files created"),
            //         give line-by-line data, such as an image's metadata (e.g. "Orientation                     : Horizontal (normal)"), or
            //         read "{ready}", which indicates that executing the last command in args.txt has completed.

            // ... do something with the information provided in line...

            if (line == null || line.Contains("{ready}"))
                break;
        }

        //  At this point, you can issue the next command for ExifTool, following the very same approach.  For instance, this tells ExifTool to
        //  extract a PreviewImage from a RAW image, using the trick of adding "%0f" at the beginning of the new filename as a way to
        //  give the PreviewImage a different name from the orginal file ...
        // args = new[]
        // {
        //     "-b", "-PreviewImage", "-w", ImagePreviewFolderPath + "%0f" + ImagePreviewFileName, RAWImageFileName,
        //     "-execute"
        // };
        // File.AppendAllLines(filename, args);

        // ... read and process _pExifTool.Value.StandardOutput again as per the above ...

        //  ... and this starts the conversion of an appropriate original image, for instance a .NEF, to .JPG:
        // args = new[]
        //     { "-b", "-JpgFromRaw", "-w", JPGImageFolderPath + "%0f" + JPGImageName, RAWImageFileName, "-execute" };
        // File.AppendAllLines(filename, args);

        // ... read and process _pExifTool.Value.StandardOutput again as per the above ...
    }

    //  Finally, this is the asynchronous error handler
    private static void ETErrorHandler(object sendingProcess, DataReceivedEventArgs errLine)
    {
        if (!string.IsNullOrEmpty(errLine.Data))
        {
            // ... do something with the information provided in errLine.Data...
        }
    }
}
