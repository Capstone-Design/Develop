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


namespace TIMPOITER
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InputInjector inputInjector = InputInjector.TryCreate();

            if (inputInjector != null)
            {
                try
                {
                    inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Indirect);

                    inputInjector.InjectTouchInput(
                        new List<InjectedInputTouchInfo>{
                            new InjectedInputTouchInfo{
                                Contact = new InjectedInputRectangle {Top = 150, Bottom = 150, Left = 40, Right = 40 },
                                PointerInfo = new InjectedInputPointerInfo{
                                    PixelLocation = new InjectedInputPoint{PositionX = 150, PositionY = 150},
                                    PointerOptions = InjectedInputPointerOptions.InContact,
                                    PointerId = 1
                                 },
                            Pressure = 1.0,
                            TouchParameters = InjectedInputTouchParameters.Pressure | InjectedInputTouchParameters.Contact
                            }
                        });
                }
                catch (ArgumentException args)
                {
                    // Handle exception.
                }
            }
        }
    }

    public class touch
    {
        public
            //vector<long, int, int> distance;
	        //vector<int, int> start;
	        //InjectedInputTouchInfo contact;   //터치 포인터 클래스 
            InputInjector inputInjector = InputInjector.TryCreate();

        private touch()
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

        void TouchInput(char type, int x, int y)
        {
            

        }

    }
}
