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

namespace CSharpCpp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.lbl.Content = string.Join(" ", buf.Select(r => r.ToString("X2")));
        }

        byte[] buf = new byte[640 * 320 * 3];
        public WriteableBitmap ImgSrc = new WriteableBitmap(640, 320, 96, 96, PixelFormats.Bgr24, null);


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            unsafe
            {
                void* p = ImgSrc.BackBuffer.ToPointer();

                TestCppDll.SetCsharpInstance s = new TestCppDll.SetCsharpInstance();
                this.img.Source = ImgSrc;
                Task.Run(() =>
                {
                    while (true)
                    {
                        //s.SetByteArrayWithRandom(buf);

                        s.FeedBackBuffer2(p);

                        Dispatcher.Invoke(() =>
                        {
                            ImgSrc.Lock();
                            ImgSrc.AddDirtyRect(new Int32Rect(0, 0, 640, 320));
                            ImgSrc.Unlock();
                        });

                        //Dispatcher.Invoke(() =>
                        //{
                        //    ImgSrc.Lock();
                        //    ImgSrc.WritePixels(new Int32Rect(0, 0, 640, 320), buf, 640 * 3, 0);
                        //    ImgSrc.Unlock();
                        //    //var tmpimg = BitmapImage.Create(640, 320, 96, 96, PixelFormats.Bgr24, BitmapPalettes.WebPalette, buf, 640 * 3);
                        //    //this.img.Source = tmpimg;
                        //});
                    }
                });

                //s.SetByteArrayWithRandom(buf);
                //this.lbl.Content = string.Join(" ", buf.Select(r => r.ToString("X2")));
            }
        }
    }
}
