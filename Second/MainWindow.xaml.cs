using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FastBitmapLib;
using Microsoft.Win32;
using static System.String;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Second
{
    public partial class MainWindow
    {
        private Bitmap _bitmap;
        private StringBuilder str;
        private Color[] _myArr;
        private Stopwatch _watch;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "Image files (*.ppm)|*.ppm"};
            if (openFileDialog.ShowDialog() != true) return;
            FileName.Text = openFileDialog.FileName;
            _watch = Stopwatch.StartNew();
            var parser = new PpmParser();
            _bitmap = parser.StartParse(openFileDialog.FileName);
            if(_bitmap == null) return;
            var ms = new MemoryStream();
            (_bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            myImage.Source = image;
            _watch.Stop();
            FileName.Text += " Time: " + _watch.ElapsedMilliseconds + " ms";
        }

        private void OpenJpegFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "Image files (*.jpeg;*.png)|*.jpeg;*.png"};
            if (openFileDialog.ShowDialog() != true) return;
            FileName.Text = openFileDialog.FileName;
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(openFileDialog.FileName);
            bitmapImage.EndInit();

            myImage.Source = bitmapImage;
        }

        private void SaveJpegFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);  
            
            var myEncoder =  
                System.Drawing.Imaging.Encoder.Quality;  
            
            var myEncoderParameters = new EncoderParameters(1);
            var result = long.TryParse(Quality.Text, out var quality);
            if (!result)
            {
                var myEncoderParameter = new EncoderParameter(myEncoder, 0L);  
                myEncoderParameters.Param[0] = myEncoderParameter;  
                _bitmap.Save(@"C:\Users\ddgam\Desktop\output.jpg", jpgEncoder, myEncoderParameters);
                MessageBox.Show("Success", "Image saved successfully", MessageBoxButton.OK);
            }
            else
            {
                var myEncoderParameter = new EncoderParameter(myEncoder, quality);  
                myEncoderParameters.Param[0] = myEncoderParameter;  
                _bitmap.Save(@"C:\Users\ddgam\Desktop\output.jpg", jpgEncoder, myEncoderParameters);
                MessageBox.Show("Success", "Image saved successfully", MessageBoxButton.OK);
            }

        }
        
        private ImageCodecInfo GetEncoder(ImageFormat format)  
        {  
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }  
    }
}