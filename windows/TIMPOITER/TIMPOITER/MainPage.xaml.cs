
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

        private SettingValue aa = new SettingValue();
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            SampleTitle.Text = FEATURE_NAME;
            aa.GetResolution();
            
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

        //public void NotifyUser(string strMessage, NotifyType type)
        //{
        //    // If called from the UI thread, then update immediately.
        //    // Otherwise, schedule a task on the UI thread to perform the update.
        //    if (Dispatcher.HasThreadAccess)
        //    {
        //        UpdateStatus(strMessage, type);
        //    }
        //    else
        //    {
        //        var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
        //    }
        //}

        //private void UpdateStatus(string strMessage, NotifyType type)
        //{
        //    switch (type)
        //    {
        //        case NotifyType.StatusMessage:
        //            StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
        //            break;
        //        case NotifyType.ErrorMessage:
        //            StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
        //            break;
        //    }

        //    StatusBlock.Text = strMessage;

        //    // Collapse the StatusBlock if it has no text to conserve real estate.
        //    StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
        //    if (StatusBlock.Text != String.Empty)
        //    {
        //        StatusBorder.Visibility = Visibility.Visible;
        //        StatusPanel.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        StatusBorder.Visibility = Visibility.Collapsed;
        //        StatusPanel.Visibility = Visibility.Collapsed;
        //    }

        //    // Raise an event if necessary to enable a screen reader to announce the status update.
        //    var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
        //    if (peer != null)
        //    {
        //        peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        //    }
        //}

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
            TextBlock textBlock = new TextBlock();
            textBlock.Text = resolution[0].ToString();
            TextBlock textBlock2 = new TextBlock();
            textBlock2.Text = resolution[1].ToString();

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
