using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoDump
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            const string Folder = // @"C:\Users\Greg\Downloads\"; 
                                  @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Folder + // "canon_eos_6d_20.CR2";
                                     // "IMG_0503.CR2";
                                     @"EOS 350D\IMG_3037.CR2";
            var stuff = new PhotoStuff(FileName2);
            var bitmap = BitmapSource.Create(stuff.Width, stuff.Height, 72, 72, PixelFormats.Gray16, null, stuff.Array, stuff.Width * 2);
            Canvas.Source = bitmap;
        }
    }
}
