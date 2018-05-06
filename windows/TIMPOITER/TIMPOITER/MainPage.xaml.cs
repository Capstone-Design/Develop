
using Microsoft.Win32;
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
        public static MainPage Current;

        
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            SampleTitle.Text = FEATURE_NAME;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Populate the scenario list from the SampleConfiguration.cs file
            ScenarioControl.ItemsSource = scenarios;
            if (Window.Current.Bounds.Width < 640)
            {
                ScenarioControl.SelectedIndex = -1;
            }
            else
            {
                ScenarioControl.SelectedIndex = 0;
            }
        }


        private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the status block when navigating scenarios.
            //NotifyUser(String.Empty, NotifyType.StatusMessage);

            ListBox scenarioListBox = sender as ListBox;
            Scenario s = scenarioListBox.SelectedItem as Scenario;
            if (s != null)
            {
                ScenarioFrame.Navigate(s.ClassType);
                if (Window.Current.Bounds.Width < 640)
                {
                    Splitter.IsPaneOpen = false;
                }
            }
        }

        public List<Scenario> Scenarios
        {
            get { return this.scenarios; }
        }

        async void Footer_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }
    }

    public class ScenarioBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Scenario s = value as Scenario;
            return (MainPage.Current.Scenarios.IndexOf(s) + 1) + ") " + s.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }



    //    private void Button_Click(object sender, RoutedEventArgs e)
    //    {
    //        //var info = new InjectedInputMouseInfo();
    //        //info.MouseOptions = InjectedInputMouseOptions.Move;
    //        //info.DeltaX = 100;

    //        //InputInjector inputInjector = InputInjector.TryCreate();
    //        //inputInjector.InjectMouseInput(new[] { info });

    //        InputInjector inputInjector = InputInjector.TryCreate();

    //        if (inputInjector != null)
    //        {
    //            try
    //            {
    //                inputInjector.InitializeTouchInjection(
    //                    InjectedInputVisualizationMode.Default);


    //                inputInjector.InjectTouchInput(
    //                    new List<InjectedInputTouchInfo>
    //                {
    //            new InjectedInputTouchInfo
    //            {
    //                Contact = new InjectedInputRectangle {
    //                    Top = 500, Bottom = 500, Left = 400, Right = 400 },
    //                PointerInfo = new InjectedInputPointerInfo
    //                {

    //                    PixelLocation = new InjectedInputPoint
    //                    {
    //                        PositionX = 400, PositionY = 500
    //                    },
    //                    PointerOptions = InjectedInputPointerOptions.InContact,
    //                    PointerId = 2
    //                },
    //                Pressure = 1.0,
    //                TouchParameters =
    //                InjectedInputTouchParameters.Pressure |
    //                InjectedInputTouchParameters.Contact
    //            }
    //                });

    //               // System.Threading.Tasks.Task.Delay(10).Wait();

    //                inputInjector.InjectTouchInput(
    //                    new List<InjectedInputTouchInfo> {
    //            new InjectedInputTouchInfo {
    //                Contact = new InjectedInputRectangle
    //                {
    //                    Top = 500,
    //                    Bottom = 500,
    //                    Left = 400,
    //                    Right = 400
    //                },
    //                PointerInfo = new InjectedInputPointerInfo {
    //                    PixelLocation = new InjectedInputPoint {
    //                        PositionX = 400, PositionY = 500
    //                    },
    //                    PointerOptions = InjectedInputPointerOptions.PointerUp,
    //                    PointerId = 2,
    //                },
    //                Pressure = 0.0,
    //                TouchParameters =
    //                InjectedInputTouchParameters.Pressure |
    //                InjectedInputTouchParameters.Contact
    //            }
    //                });
    //            }
    //            catch (ArgumentException args)
    //            {

    //            }
    //        }
    //    }
    //}

    public class SettingValue
    {
        private static SettingValue instance;
        private static int[] screensize = new int[2];
        private static int[] resolution = new int[2];
        private static double[] calibration = new double[2];
        //arduino = new [2];
        private int firstStart;

        private SettingValue() { }

        public static SettingValue GetInstance()
        {
            if(instance == null)
            {
                instance = new SettingValue();
            }
            return instance;
        }

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
        public int[] GetResolution()
        {
            resolution[0] = (int)DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
            resolution[1] = (int)DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;
            return resolution;
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
