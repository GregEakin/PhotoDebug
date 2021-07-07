// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		MainWindow.cs
// AUTHOR:		Greg Eakin

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
			
            const string folder = @"P:\2016\2016-02-21 Studio\";
            const string fileName2 = folder + "Studio 015.CR2";
            var stuff = new PhotoStuff(fileName2);
            // var bitmap = stuff.Array;
            // Canvas.Source = (Image)bitmap;
        }
    }
}
