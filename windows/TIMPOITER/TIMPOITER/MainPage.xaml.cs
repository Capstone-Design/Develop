
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;

namespace TIMPOITER
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SettingValue aa = new SettingValue();
        public MainPage()
        {
            this.InitializeComponent();

            aa.GetResolution();
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var info = new InjectedInputMouseInfo();
            //info.MouseOptions = InjectedInputMouseOptions.Move;
            //info.DeltaX = 100;

            //InputInjector inputInjector = InputInjector.TryCreate();
            //inputInjector.InjectMouseInput(new[] { info });

            InputInjector inputInjector = InputInjector.TryCreate();
            
            if (inputInjector != null)
            {
                try
                {
                    inputInjector.InitializeTouchInjection(
                        InjectedInputVisualizationMode.Default);

                   
                    inputInjector.InjectTouchInput(
                        new List<InjectedInputTouchInfo>
                    {
                new InjectedInputTouchInfo
                {
                    Contact = new InjectedInputRectangle {
                        Top = 500, Bottom = 500, Left = 400, Right = 400 },
                    PointerInfo = new InjectedInputPointerInfo
                    {
                        
                        PixelLocation = new InjectedInputPoint
                        {
                            PositionX = 400, PositionY = 500
                        },
                        PointerOptions = InjectedInputPointerOptions.InContact,
                        PointerId = 2
                    },
                    Pressure = 1.0,
                    TouchParameters =
                    InjectedInputTouchParameters.Pressure |
                    InjectedInputTouchParameters.Contact
                }
                    });
                    
                   // System.Threading.Tasks.Task.Delay(10).Wait();

                    inputInjector.InjectTouchInput(
                        new List<InjectedInputTouchInfo> {
                new InjectedInputTouchInfo {
                    Contact = new InjectedInputRectangle
                    {
                        Top = 500,
                        Bottom = 500,
                        Left = 400,
                        Right = 400
                    },
                    PointerInfo = new InjectedInputPointerInfo {
                        PixelLocation = new InjectedInputPoint {
                            PositionX = 400, PositionY = 500
                        },
                        PointerOptions = InjectedInputPointerOptions.PointerUp,
                        PointerId = 2,
                    },
                    Pressure = 0.0,
                    TouchParameters =
                    InjectedInputTouchParameters.Pressure |
                    InjectedInputTouchParameters.Contact
                }
                    });
                }
                catch (ArgumentException args)
                {
                    
                }
            }
        }
    }

    public class SettingValue
    {

        private int[] screensize = new int[2];
        private int[] resolution = new int[2];
        private double[] calibration = new double[2];
        //arduino = new [2];
        private int firstStart;

        //해상도가 변경되었을 때의 설정 변경
        public void ResolutionChanged()
        {

        }

        //디스플레이 사이즈 저장 
        public void SetScreen(int[] size)
        {
            screensize[0] = size[0];
            screensize[1] = size[1];
        }

        // 해상도 가져오기 및 저장 
        public Size GetResolution()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            resolution[0] = (int)size.Height;
            resolution[1] = (int)size.Width;
 
            return size;
        }

        /*public int* GetBattery()
        {

            return &; 
        }*/
    }

    public class Touch
    {
        // public
        //vector<long, int, int> distance;
        //vector<int, int> start;
        //InjectedInputTouchInfo contact;   //터치 포인터 클래스 
        //InputInjector inputInjector = InputInjector.TryCreate();

        public Touch()
        {
            //inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Indirect);
            //inputInjector.InjectTouchInput(new List<InjectedInputTouchInfo>
            //{
            //    new InjectedInputTouchInfo
            //    {
            //        Contact = new InjectedInputRectangle
            //        {
            //            Top = 50, Bottom = 50, Left = 40, Right = 40
            //        },

            //        PointerInfo = new InjectedInputPointerInfo
            //        {
            //            PixelLocation = new InjectedInputPoint
            //            {
            //                PositionX = 40, PositionY = 50
            //            },

            //            PointerOptions = InjectedInputPointerOptions.InContact,
            //            PointerId = 1
            //        },

            //        Pressure = 1.0,
            //        TouchParameters = InjectedInputTouchParameters.Pressure | InjectedInputTouchParameters.Contact

            //    }
            //});
            
        }

        public void TouchInput(char type, int x, int y)
        {
            

        }

    }
}
