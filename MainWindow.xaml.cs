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

namespace openCvWinCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public WriteableBitmap ImgSrc = new WriteableBitmap(640, 360, 96, 96, PixelFormats.Bgr24, null);
        opencvwrapper.OpenCvWrapper obj = new opencvwrapper.OpenCvWrapper();
        byte[] imgbuffer = new byte[640 * 320 * 3];

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.img.Source = ImgSrc;
            this.lbl.Content = obj.SayHelloWorld();
            
            Task.Run(() =>
            {
                var res = obj.OpenCamera();

                Dispatcher.Invoke(() => this.log.AppendText($"camera open result : {res} {Environment.NewLine}"));

                

                while (res)
                {
                    var returnvalue = obj.FeedImg(imgbuffer);

                    Dispatcher.Invoke(() => this.log.AppendText($"return value : {returnvalue} {Environment.NewLine}"));

                    Dispatcher.Invoke(() => {
                        var tmpimg = BitmapImage.Create(640, 320, 96, 96, PixelFormats.Bgr24, BitmapPalettes.WebPalette, imgbuffer, 640 * 3);
                        this.img.Source = tmpimg;
                    });

                    //ImgSrc.Lock();
                    //ImgSrc.WritePixels(new Int32Rect(0, 0, 640, 320), imgbuffer, 640 * 3, 0);
                    //ImgSrc.Unlock();

                    //var image = obj.ReadImage2();
                    //Dispatcher.Invoke(() => this.log.AppendText($"image read : {image} {Environment.NewLine}"));

                    //if (image != null)
                    //{
                    //    ImgSrc.WritePixels(new Int32Rect(0, 0, 640, 320), image, 640 * 3, 0);
                    //}

                    //if (image != null)
                    //{
                    //    Dispatcher.Invoke(() => this.log.AppendText($"{image.Height} {image.Width} {Environment.NewLine}"));
                    //}
                    //else
                    //{
                    //    Dispatcher.Invoke(() => this.log.AppendText($"image null {Environment.NewLine}"));
                    //}

                    //Dispatcher.Invoke(() => this.img.Source = image);
                }
            });
        }
    }
}
