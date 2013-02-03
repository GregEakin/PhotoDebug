using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            const string Folder = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Folder + "IMG_0503.CR2";
            var stuff = new PhotoStuff(FileName2);
            var bitmap = BitmapSource.Create(stuff.Width, stuff.Height, 96, 96, PixelFormats.Gray16, null, stuff.Array, stuff.Width * 2);
            Canvas.Source = bitmap;
        }
    }
}
