using System;
using System.Collections.Generic;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace UWP_TouchInjection_Test
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private InputInjector inputInjector;
        public MainPage()
        {
            this.InitializeComponent();

            /* Package.appmanifest에 
             * <Package xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
             *          IgnorableNamespaces="uap mp rescap">
             * <Capabilities>
             *    <rescap:Capability Name="inputInjectionBrokered" />
             * </Capabilities>
             * 두 항목 추가해야 인젝션이 제대로 전달됨.
             */

            // Init 
            inputInjector = InputInjector.TryCreate();
            // Default는 기본 터치 이펙트, Indirect는 원에 +표시가 있는 마크가 표시됨.
            inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Default);

        }

        private void Inject_Button_Click(object sender, RoutedEventArgs e)
        {
            // 더블 터치
            Touch(1, 150, 170);
            System.Threading.Tasks.Task.Delay(100).Wait();
            Touch(2, 150, 170);
            System.Threading.Tasks.Task.Delay(100).Wait();
            Touch(1, 150, 170);
            System.Threading.Tasks.Task.Delay(100).Wait();
            Touch(2, 150, 170);

            //드래그
            //Touch(1, 100, 100);
            //for(var i = 0; i < 100; i++)
            //{
            //    System.Threading.Tasks.Task.Delay(100).Wait();
            //    Touch(0, 100 + (double)(500 - 100) / 100 * i, 100 + (double)(500 - 100) / 100 * i);
            //}
            //Touch(2, 500, 500);


        }
        private int Touch(int type, double X, double Y)
        {
            InjectedInputPointerOptions pointerOption;
            Double pressure;        // 0.0~ 1.0 (1024단계)
            switch (type)
            {
                case 0:
                    pointerOption = InjectedInputPointerOptions.InRange;
                    pressure = 1.0;
                    break;
                case 1:
                    pointerOption = InjectedInputPointerOptions.InContact;
                    pressure = 1.0;
                    break;
                case 2:
                    pointerOption = InjectedInputPointerOptions.PointerUp;
                    pressure = 0;
                    break;
                default:
                    return -1;
            }
            var location = new InjectedInputPoint { PositionX = (int)X, PositionY = (int)Y };

            try
            {
                inputInjector.InjectTouchInput
                (
                    new List<InjectedInputTouchInfo>
                    {
                        new InjectedInputTouchInfo
                        {
                            Contact = new InjectedInputRectangle
                            {
                                Top = 50,
                                Bottom = 50,
                                Left = 40,
                                Right = 40
                            },
                            PointerInfo = new InjectedInputPointerInfo
                            {
                                PixelLocation = location,
                                PointerOptions = pointerOption,
                                PointerId = 1,
                            },
                            Pressure = pressure,
                            TouchParameters =
                            InjectedInputTouchParameters.Pressure |
                            InjectedInputTouchParameters.Contact
                        }
                    }
                );
                return 1;
            }
            catch (Exception e)
            {
                return -2;
            }
        }

    }

}
