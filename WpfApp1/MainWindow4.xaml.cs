using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow4.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow4 : Window
    {
        public MainWindow4()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //1
            WpfDrawingSettings settings = new WpfDrawingSettings();
            settings.IncludeRuntime = true;
            settings.TextAsGeometry = false;

            //2
            string svgTestFile = @"./Images/SVG111.svg";

            //3 Create File Reader
            var converter = new FileSvgReader(settings);

            //4 Read the SVG file
            DrawingGroup drawing = converter.Read(svgTestFile);

            if(drawing != null )
            {
                svgImage.Source = new DrawingImage(drawing);

                using(StringWriter textWriter = new StringWriter())
                {
                    if(converter.Save(textWriter))
                    {
                        svgBox.Text = textWriter.ToString();
                    }
                }


                // 5. Perform the conversion to image  
                ImageSvgConverter converter2 = new ImageSvgConverter(settings);
                converter2.EncoderType = ImageEncoderType.BmpBitmap;
                //converter2.Convert(svgTestFile);

                converter2.EncoderType = ImageEncoderType.GifBitmap;
                //converter2.Convert(svgTestFile);

                converter2.EncoderType = ImageEncoderType.JpegBitmap;
                //converter2.Convert(svgTestFile);

                converter2.EncoderType = ImageEncoderType.PngBitmap;
                //converter2.Convert(svgTestFile);

                converter2.EncoderType = ImageEncoderType.TiffBitmap;
                //converter2.Convert(svgTestFile);

                converter2.EncoderType = ImageEncoderType.WmpBitmap;
                //converter2.Convert(svgTestFile);


                //6. StreamImageConverter
                StreamSvgConverter converter3 = new StreamSvgConverter(settings);
                using (MemoryStream memStream = new MemoryStream())
                {
                    if (converter3.Convert(svgTestFile, memStream))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memStream;
                        bitmap.EndInit();
                        // Set the image source.
                        svgImageBmp.Source = bitmap;
                    }
                }

                //7. FileSvgConverter
                FileSvgConverter converter4 = new FileSvgConverter(settings);
                //8. Convert to Xaml
                converter4.Convert(svgTestFile);
            }


        }
    }
}
