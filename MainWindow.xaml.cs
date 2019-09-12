using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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

        //[DllImport("user32.dll")]
        //public static extern IntPtr GetDC(IntPtr hWnd);
        //[DllImport("gdi32.dll")]
        //public static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);
        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public struct RAMP
        //{
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        //    public UInt16[] Red;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        //    public UInt16[] Green;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        //    public UInt16[] Blue;
        //}
        //public static void SetGamma(int gamma)
        //{
        //    if (gamma <= 256 && gamma >= 1)
        //    {
        //        RAMP ramp = new RAMP();
        //        ramp.Red = new ushort[256];
        //        ramp.Green = new ushort[256];
        //        ramp.Blue = new ushort[256];
        //        for (int i = 1; i < 256; i++)
        //        {
        //            int iArrayValue = i * (gamma + 128);

        //            if (iArrayValue > 65535)
        //                iArrayValue = 65535;
        //            ramp.Red[i] = ramp.Blue[i] = ramp.Green[i] = (ushort)iArrayValue;
        //        }
        //        SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
        //    }
        //}

        public WriteableBitmap ImgSrc = new WriteableBitmap(1280, 720, 96, 96, PixelFormats.Bgr24, null);

        opencvwrapper.OpenCvWrapper obj = new opencvwrapper.OpenCvWrapper();

        string currentPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        byte[] imgbuffer = new byte[1280 * 720 * 3];

        string imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        public void SaveImageToFile(BitmapSource image)
        {
            string filename = Guid.NewGuid().ToString();
            using (var fileStream = new FileStream(System.IO.Path.Combine(imagePath, filename + ".png"), FileMode.CreateNew))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
            using (var fileStream = new FileStream(System.IO.Path.Combine(imagePath, filename + ".jpeg"), FileMode.CreateNew))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 90;
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        //const int MONITOR_DEFAULTTONULL = 0;
        //const int MONITOR_DEFAULTTOPRIMARY = 1;
        //const int MONITOR_DEFAULTTONEAREST = 2;
        //[DllImport("user32.dll")]
        //static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        //[DllImport("user32.dll", SetLastError = false)]
        //static extern IntPtr GetDesktopWindow();
        //private const int PHYSICAL_MONITOR_DESCRIPTION_SIZE = 128;
        //[StructLayout(LayoutKind.Sequential)]
        //public struct PHYSICAL_MONITOR
        //{
        //    public IntPtr hPhysicalMonitor;
        //    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = PHYSICAL_MONITOR_DESCRIPTION_SIZE)]
        //    public char[] szPhysicalMonitorDescription;
        //}
        //[DllImport("dxva2.dll", SetLastError = true)]
        //private extern static bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);
        //[DllImport("dxva2.dll", SetLastError = true)]
        //private extern static bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);
        //[DllImport("dxva2.dll", SetLastError = true)]
        //private extern static bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);
        //[DllImport("dxva2.dll", SetLastError = true)]
        //private extern static bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

        private void l(object obj)
        {
            Dispatcher.Invoke(() =>
            {
                log.AppendText($"{obj}{Environment.NewLine}");
                log.ScrollToEnd();
            });
        }

        bool isStarted = false;
        string xmlpath = "";

        public volatile bool shouldStopCamera = false;

        private Task lastTask = null;

        private Task startCamera()
        {
            if (isStarted || (lastTask != null && lastTask.IsCompleted != true))
            {
                throw new Exception("last task is running");
            }

            if (String.IsNullOrWhiteSpace(xmlpath)) return null;

            isStarted = true;

            return Task.Run(() =>
            {
                var res = obj.OpenCamera(currentCamId);

                l($"camera open result : camid -> {currentCamId} {res} ");

                var res2 = obj.LoadXml(System.IO.Path.Combine(currentPath, xmlpath));

                l($"xml load result : xmlpath -> {xmlpath} {res2} ");

                int loopcnt = 1;

                while (res && shouldStopCamera == false)
                {
                    loopcnt++;
                    //var returnvalue = obj.FeedImg2(imgbuffer);

                    var returnvalue = obj.FeedImg3(imgbuffer, flipFlag, imageScale, neighbor, scaleFractor, minSize, maxSize);

                    //l($"return value : {returnvalue}");

                    if (shouldStopCamera == false)
                        Dispatcher.Invoke(() =>
                        {
                            //l($"Before ActualHeight:{this.img.ActualHeight} ActualWidth:{this.img.ActualWidth} Height:{this.img.Height} Width:{this.img.Width}");
                            var tmpimg = BitmapImage.Create(1280, 720, 96, 96, PixelFormats.Bgr24, BitmapPalettes.WebPalette, imgbuffer, 1280 * 3);
                            this.img.Source = tmpimg;
                            this.canvas.Children.Clear();
                            if (returnvalue != null && returnvalue.Length > 0)
                            {
                                int lastx = 0, lasty = 0;
                                for (int i = 0; i < returnvalue.Length / 4; i++)
                                {
                                    var rect = new Rectangle
                                    {
                                        Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
                                        StrokeThickness = 4,
                                        Width = returnvalue[i * 4 + 2],
                                        Height = returnvalue[i * 4 + 3]
                                    };
                                    Canvas.SetLeft(rect, returnvalue[i * 4]);
                                    Canvas.SetTop(rect, returnvalue[i * 4 + 1]);
                                    canvas.Children.Add(rect);
                                    if (returnvalue.Length == 4)
                                    {
                                        lastx = returnvalue[i * 4] + returnvalue[i * 4 + 2] / 2;
                                        lasty = returnvalue[i * 4 + 1] + returnvalue[i * 4 + 3] / 2;
                                    }
                                }

                                if (returnvalue.Length == 4 && Math.Abs(lastx - 640) < 10 && Math.Abs(lasty - 360) < 10)
                                {
                                    if (MessageBox.Show(this,$"十字に当たる画像が検知しましたが、{Environment.NewLine}保存しますか？","画像保存確認", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                                        SaveImageToFile(tmpimg);
                                    //var tsk = Task.Run(() => SaveImageToFile(tmpimg));
                                    //tsk.Wait();
                                }
                            }
                            //l($"After ActualHeight:{this.img.ActualHeight} ActualWidth:{this.img.ActualWidth} Height:{this.img.Height} Width:{this.img.Width}");
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

                if (shouldStopCamera)
                {
                    l($"camera close : camid -> {currentCamId} ");
                    obj.CloseCamera();
                }
            });
        }

        int brightness = 50;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.log.Text = "";

            //SetGamma(254);

            this.img.Source = ImgSrc;
            //this.canvas.Height = this.img.Height;
            //this.canvas.Width = this.img.Width;

            //set hardware brightness (like press ajust button on display)
            //not work on notebook and tablet
            //IntPtr desktopMonitor = MonitorFromWindow(GetDesktopWindow(), MONITOR_DEFAULTTONEAREST);
            //IntPtr currentWindowMonitor = MonitorFromWindow(new WindowInteropHelper(this).Handle, MONITOR_DEFAULTTONEAREST);
            //System.Diagnostics.Debug.WriteLine($"{desktopMonitor} {currentWindowMonitor}");
            //uint dwNumberOfPhysicalMonitors;
            //if (GetNumberOfPhysicalMonitorsFromHMONITOR(currentWindowMonitor, out dwNumberOfPhysicalMonitors))
            //{
            //    PHYSICAL_MONITOR[] physicalMonitorArray = new PHYSICAL_MONITOR[dwNumberOfPhysicalMonitors];
            //    if (GetPhysicalMonitorsFromHMONITOR(currentWindowMonitor, dwNumberOfPhysicalMonitors, physicalMonitorArray))
            //    {
            //        uint dwMinimumBrightness, dwCurrentBrightness, dwMaximumBrightness;
            //        if (GetMonitorBrightness(physicalMonitorArray[0].hPhysicalMonitor, out dwMinimumBrightness, out dwCurrentBrightness, out dwMaximumBrightness))
            //        {
            //            System.Diagnostics.Debug.WriteLine($"{dwMinimumBrightness} {dwCurrentBrightness} {dwMaximumBrightness}");
            //            //MessageBox.Show($"{dwMinimumBrightness} {dwCurrentBrightness} {dwMaximumBrightness}");
            //        }
            //        else
            //        {
            //            System.Diagnostics.Debug.WriteLine($"GetMonitorBrightness false");
            //            //MessageBox.Show($"GetMonitorBrightness false");
            //        }
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine($"GetPhysicalMonitorsFromHMONITOR false");
            //        //MessageBox.Show($"GetPhysicalMonitorsFromHMONITOR false");
            //    }
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine($"GetNumberOfPhysicalMonitorsFromHMONITOR false");
            //    //MessageBox.Show($"GetNumberOfPhysicalMonitorsFromHMONITOR false");
            //}

            //create a management scope object
            ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\WMI");
            //create object query
            ObjectQuery query = new ObjectQuery("SELECT * FROM WmiMonitorBrightness");
            //create object searcher
            ManagementObjectSearcher searcher =
                                    new ManagementObjectSearcher(scope, query);
            //get a collection of WMI objects
            ManagementObjectCollection queryCollection = searcher.Get();
            //enumerate the collection.
            foreach (ManagementObject m in queryCollection)
            {
                // access properties of the WMI object
                System.Diagnostics.Debug.WriteLine("CurrentBrightness : {0}", m["CurrentBrightness"]);

                int.TryParse("" + m["CurrentBrightness"], out brightness);

            }

            //set the class name and namespace
            string NamespacePath = "\\\\.\\ROOT\\WMI";
            string ClassName = "WmiMonitorBrightnessMethods";

            //Create ManagementClass
            ManagementClass oClass = new ManagementClass(NamespacePath + ":" + ClassName);
            foreach (ManagementObject mo in oClass.GetInstances())
            {
                ManagementBaseObject inParams = mo.GetMethodParameters("WmiSetBrightness");
                inParams["Brightness"] = 100; // 輝度を level % に
                inParams["Timeout"] = 1;       // 操作のタイムアウトを 5 秒にセット
                mo.InvokeMethod("WmiSetBrightness", inParams, null);
            }

            var res = obj.ListCameras();
            l($"Camera Ids [{string.Join(",", res)}]");
            //foreach (var tmp in res)
            //{
            //    if (tmp >= 0)
            //    {
            //        if (!camIds.Contains(tmp))
            //            camIds.Add(tmp);
            //    }
            //}

            camIds.Add(0);
            camIds.Add(1);

            if (camIds.Count > 0)
                currentCamId = camIds[0];

            l($"Cam Count [{camIds.Count}]");

            m1.IsChecked = true;

            if (camIds.Count <= 1)
            {
                this.swt.IsEnabled = false;
            }

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Dispatcher.Invoke(() => controlPanel.Visibility = Visibility.Collapsed);
            });
        }

        private List<int> camIds = new List<int>();
        int currentCamId = -1;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            //set the class name and namespace
            string NamespacePath = "\\\\.\\ROOT\\WMI";
            string ClassName = "WmiMonitorBrightnessMethods";

            //Create ManagementClass
            ManagementClass oClass = new ManagementClass(NamespacePath + ":" + ClassName);
            foreach (ManagementObject mo in oClass.GetInstances())
            {
                ManagementBaseObject inParams = mo.GetMethodParameters("WmiSetBrightness");
                inParams["Brightness"] = brightness; // 輝度を level % に
                inParams["Timeout"] = 1;       // 操作のタイムアウトを 5 秒にセット
                mo.InvokeMethod("WmiSetBrightness", inParams, null);
            }
        }

        private void setLogStyle(object sender, RoutedEventArgs e)
        {
            //if (this.log.Height == 50)
            //{
            //    this.log.Height = 700;
            //}
            //else
            //{
            //    this.log.Height = 50;
            //}
            if (controlPanel.Visibility != Visibility.Visible)
            {
                controlPanel.Visibility = Visibility.Visible;
            }
            else
            {
                controlPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void closeApp(object sender, RoutedEventArgs e)
        {
            //Marshal.ReleaseComObject(obj);
            this.Close();
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            if (lastTask == null)
            {
                l("Switch_Click lastTask is null");

                shouldStopCamera = false;

                int idx = camIds.IndexOf(currentCamId);

                if (idx == camIds.Count - 1)
                {
                    currentCamId = 0;
                }
                else
                {
                    currentCamId = camIds[idx + 1];
                }

                lastTask = startCamera();
            }
            else
            {
                shouldStopCamera = true;

                Task.Run(() =>
                {
                    l("Switch_Click begin wait");
                    lastTask.Wait();
                    l("Switch_Click finish wait");

                    isStarted = false;
                    lastTask.Dispose();
                    lastTask = null;

                    shouldStopCamera = false;

                    int idx = camIds.IndexOf(currentCamId);

                    if (idx == camIds.Count - 1)
                    {
                        currentCamId = 0;
                    }
                    else
                    {
                        currentCamId = camIds[idx + 1];
                    }

                    lastTask = startCamera();
                });

            }
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            if (lastTask == null)
            {
                xmlpath = "haarcascade_frontalface_default.xml";
                shouldStopCamera = false;
                lastTask = startCamera();
            }
            else
            {
                shouldStopCamera = true;
                Task.Run(() =>
                {
                    lastTask.Wait();
                    xmlpath = "haarcascade_frontalface_default.xml";
                    isStarted = false;
                    lastTask.Dispose();
                    lastTask = null;

                    shouldStopCamera = false;

                    lastTask = startCamera();
                });
            }
        }
        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (lastTask == null)
            {
                xmlpath = "haarcascade_frontalface_alt.xml";
                shouldStopCamera = false;
                lastTask = startCamera();
            }
            else
            {
                shouldStopCamera = true;
                Task.Run(() =>
                {
                    lastTask.Wait();
                    xmlpath = "haarcascade_frontalface_alt.xml";
                    isStarted = false;
                    lastTask.Dispose();
                    lastTask = null;

                    shouldStopCamera = false;

                    lastTask = startCamera();
                });
            }
        }
        private void RadioButton3_Checked(object sender, RoutedEventArgs e)
        {
            if (lastTask == null)
            {
                xmlpath = "haarcascade_frontalface_alt2.xml";
                shouldStopCamera = false;
                lastTask = startCamera();
            }
            else
            {
                shouldStopCamera = true;
                Task.Run(() =>
                {
                    lastTask.Wait();
                    xmlpath = "haarcascade_frontalface_alt2.xml";
                    isStarted = false;
                    lastTask.Dispose();
                    lastTask = null;

                    shouldStopCamera = false;

                    lastTask = startCamera();
                });
            }
        }
        private void RadioButton4_Checked(object sender, RoutedEventArgs e)
        {
            if (lastTask == null)
            {
                xmlpath = "haarcascade_frontalface_alt_tree.xml";
                shouldStopCamera = false;
                lastTask = startCamera();
            }
            else
            {
                shouldStopCamera = true;
                Task.Run(() =>
                {
                    lastTask.Wait();
                    xmlpath = "haarcascade_frontalface_alt_tree.xml";
                    isStarted = false;
                    lastTask.Dispose();
                    lastTask = null;

                    shouldStopCamera = false;

                    lastTask = startCamera();
                });
            }
        }

        public volatile bool flipFlag = false;
        public volatile int imageScale = 4;
        public volatile float scaleFractor = 1.1F;
        public volatile int minSize = 30;
        public volatile int maxSize = 180;
        public volatile int neighbor = 2;

        private void setFlip(object sender, RoutedEventArgs e)
        {
            if (this.ckbFlip.IsChecked == true)
            {
                flipFlag = true;
            }
            else
            {
                flipFlag = false;
            }
        }

        private void imageScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double v = slider.Value;

            imageScale = (int)v;
        }

        private void scaleFractorChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double v = slider.Value;

            scaleFractor = (float)(1 + v / 10);
        }

        private void minChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double v = slider.Value;

            minSize = (int)v;
        }

        private void maxChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double v = slider.Value;

            maxSize = (int)v;
        }

        private void neighborChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            double v = slider.Value;

            neighbor = (int)v;
        }


        //private void switchTaskA(object sender, RoutedEventArgs e)
        //{
        //    if (ltk == null)
        //    {
        //        shouldStop = false;
        //        ltk = Task.Run(() =>
        //        {
        //            while (shouldStop != true)
        //            {
        //                System.Diagnostics.Debug.WriteLine("......................");
        //            }
        //        });
        //    }
        //    else
        //    {
        //        shouldStop = true;
        //        ltk.Wait();
        //        shouldStop = false;
        //        ltk = Task.Run(() =>
        //        {
        //            while (shouldStop != true)
        //            {
        //                System.Diagnostics.Debug.WriteLine("......................");
        //            }
        //        });
        //    }
        //}

        //private Task ltk;
        //private bool shouldStop = false;

        //private void switchTaskB(object sender, RoutedEventArgs e)
        //{
        //    if (ltk == null)
        //    {
        //        shouldStop = false;
        //        ltk = Task.Run(() =>
        //        {
        //            while (shouldStop != true)
        //            {
        //                System.Diagnostics.Debug.WriteLine("###################");
        //            }
        //        });
        //    }
        //    else
        //    {
        //        shouldStop = true;
        //        ltk.Wait();
        //        shouldStop = false;
        //        ltk = Task.Run(() =>
        //        {
        //            while (shouldStop != true)
        //            {
        //                System.Diagnostics.Debug.WriteLine("###################");
        //            }
        //        });
        //    }
        //}

        //private void startTask(string msg)
        //{
        //    ltk = Task.Run(async () =>
        //    {
        //        while (shouldStop != true)
        //        {
        //            await Task.Delay(200);
        //            l(msg);
        //        }
        //    });
        //}

        //private void switchTaskC(object sender, RoutedEventArgs e)
        //{
        //    if (ltk == null)
        //    {
        //        shouldStop = false;
        //        ltk = Task.Run(async () =>
        //        {
        //            Random rnd = new Random(DateTime.Now.Millisecond);
        //            var msg = $"{rnd.Next()} {rnd.Next()} {rnd.Next()}";
        //            while (shouldStop != true)
        //            {
        //                await Task.Delay(200);
        //                l(msg);
        //            }
        //        });
        //    }
        //    else
        //    {
        //        shouldStop = true;
        //        Task.Run(() =>
        //        {
        //            ltk.Wait();
        //            shouldStop = false;
        //            ltk = Task.Run(async () =>
        //            {
        //                Random rnd = new Random(DateTime.Now.Millisecond);
        //                var msg = $"{rnd.Next()} {rnd.Next()} {rnd.Next()}";
        //                while (shouldStop != true)
        //                {
        //                    await Task.Delay(200);
        //                    l(msg);
        //                }
        //            });
        //        });
        //        //ltk.Wait();

        //    }
        //}
    }
}
