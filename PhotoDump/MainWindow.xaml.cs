// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		DumpImageSlices.cs
// AUTHOR:		Greg Eakin

using System.Windows;

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

            const string folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            const string fileName2 = folder + "Studio 015.CR2";
            var stuff = new PhotoStuff(fileName2);
            // var bitmap = stuff.Array;
            // Canvas.Source = (Image)bitmap;
        }
    }
}
