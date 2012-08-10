using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ValidateCodeRecognize.Core;

namespace WpfApplication1
{
    /// <summary>
    /// </summary>
    public static class BitmapConversion
    {
        /// <summary>
        /// </summary>
        /// <param name="bitmapsource"></param>
        /// <returns></returns>
        public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new Bitmap(tempBitmap);
                }
            }
        }

        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            engine = new ValidateCodeRecognizeEngine();
        }

        private ValidateCodeRecognizeEngine engine;
        private List<Bitmap> ToLearnBitmaps;
        private Bitmap _toRecognizeBitmap;

        private void click_Click_1(object sender, RoutedEventArgs e)
        {
            this.ToLearnBitmaps = this.engine.GetToLearnBitmaps();
            for (int i = 0; i < this.ToLearnBitmaps.Count; i++)
            {
                var wpfBitmap = this.ToLearnBitmaps[i].ToWpfBitmap();
                switch (i)
                {
                    case 0:
                        this.Image1.Source = wpfBitmap;
                        break;
                    case 1:
                        this.Image2.Source = wpfBitmap;
                        break;
                    case 2:
                        this.Image3.Source = wpfBitmap;
                        break;
                    case 3:
                        this.Image4.Source = wpfBitmap;
                        break;
                }
            }
        }



        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var value = this.TextBox1.Text;
            for (int i = 0; i < this.ToLearnBitmaps.Count; i++)
            {
                var bitmap = this.ToLearnBitmaps[i];
                this.engine.Learn(bitmap, value[i].ToString());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this._toRecognizeBitmap = this.engine.GetToRecognizeBitmap();
            this.ImageToReco.Source = this._toRecognizeBitmap.ToWpfBitmap();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var deltadifference = Convert.ToInt32(this.diffenet.Text);
            var delta = Convert.ToInt32(this.delta.Text);
            this.textBoxReco.Text = this.engine.Recognize(_toRecognizeBitmap, 4, delta, deltadifference);
        }
    }
}
