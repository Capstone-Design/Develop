
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;

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

            SystemNavigationManagerPreview mgr =
                SystemNavigationManagerPreview.GetForCurrentView();
            mgr.CloseRequested += SystemNavigationManager_CloseRequested;
            Systrayicon();
            //BackBaseTask();
        }

        //테스트 용 컴퓨터 시간대를 변경시 실행 
        public void BackBaseTask()
        {
            var taskRegistered = false;
            var exampleTaskName = "BaseTask";

            foreach (var task1 in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (task1.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "BackgroundTask.BaseTask";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));

                builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));

                BackgroundTaskRegistration task = builder.Register();

                task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
            }

        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("back ground task sss");

            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var key = task.TaskId.ToString();
            var message = settings.Values[key].ToString();
            System.Diagnostics.Debug.WriteLine(message);
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

        // 트레이 아이콘 생성 
        public async void Systrayicon()
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }

            
        }

        
        // X 버튼 누르시에 할 일을 추가해야함 
        private void SystemNavigationManager_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            Deferral deferral = e.GetDeferral();
            
            
            e.Handled = false;
            deferral.Complete();
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



 

    public class SettingValue
    {
        private static SettingValue instance;

        //로컬데이터에 저장 
        ApplicationDataContainer localSettings = null;
        private static int[] screensize = new int[2];
        private static int[] resolution = new int[2];
        private static double[] calibration = new double[2];
        

        //arduino = new [2];
        //private int firstStart;

        private SettingValue()
        {
            localSettings = ApplicationData.Current.LocalSettings;

            if(localSettings != null)
            {
                screensize[0] = Convert.ToInt32(localSettings.Values["screensizeW"]);
                screensize[1] = Convert.ToInt32(localSettings.Values["screensizeH"]);
                resolution[0] = Convert.ToInt32(localSettings.Values["resolutionW"]);
                resolution[1] = Convert.ToInt32(localSettings.Values["resolutionH"]);

            }
        }

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
        public void SetScreenSize(int[] size)
        {
            screensize[0] = size[0];
            screensize[1] = size[1];
            localSettings.Values["screensizeW"] = screensize[0];
            localSettings.Values["screensizeH"] = screensize[1];
        }

        public int[] GetScreenSize()
        {
            return screensize;
        }

        // 해상도 가져오기 및 저장 
        public int[] GetResolution()
        {
            resolution[0] = (int)DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
            resolution[1] = (int)DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;
            localSettings.Values["resolutionW"] = resolution[0];
            localSettings.Values["resolutionH"] = resolution[1];

            return resolution;
        }

        /*public int* GetBattery()
        {

            return &; 
        }*/
    }

    public class Touch
    {
        private static Touch instance;

        // public
        int preX;
        int preY;
        //private JsonObject distance1;
        //private JsonObject distance2;
        //private JsonObject data;

        InputInjector inputInjector;
        bool pointerupdate = false; // move와 touchup 구분 변수

        public Touch()
        {
            inputInjector = InputInjector.TryCreate();
            inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Default);
        }

        public static Touch GetInstance()
        {
            if (instance == null)
            {
                instance = new Touch();
            }
            return instance;
        }

        public int TouchInput(int type, int X, int Y)
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

        // 타입 분류 
        public void CalculateTouch(int x, int y)
        {
            
            if(pointerupdate == true)
            {
                if((x == -1)&&(y == -1)) // touchup
                {
                    TouchInput(2, preX, preY);
                    pointerupdate = false;
                }
                else  
                {
                    TouchInput(0, x, y);   // move
                }
            }
            else  //  tab
            {
                TouchInput(1, x,y);
                pointerupdate = true;
            }
            preX = x;
            preY = y;
        }

        public void PutDistance(JsonObject distance1, JsonObject distance2)
        {
            int[] screensize = SettingValue.GetInstance().GetScreenSize();
            int[] resolution = SettingValue.GetInstance().GetResolution();
            int d1 = 9000; // 왼쪽 센서의 거리 값 
            int d2 = 9000; // 오른쪽 센서의 거리 값
            int temp = 0; // 임시 변수 
            int k = 45; // 센서와 모서리의 거리 값 

            //왼쪽 센서 거리 값 결정
            for(int i = 0; i < distance1["distance"].GetArray().Count; i++)
            {
                temp = (int)distance1["distance"].GetArray()[i].GetNumber();
                if (d1 > temp)
                {
                    if(i == 3)
                    {
                        if(screensize[0] > (temp - k))
                        {
                            d1 = temp;
                        }
                    }
                    else
                    {
                        d1 = temp;
                    }
                }
            }
            //오른쪽 센서 거리 값 결정 
            for (int i = 0; i < distance2["distance"].GetArray().Count; i++)
            {
                temp = (int)distance2["distance"].GetArray()[i].GetNumber();
                if (d1 > temp)
                {
                    if (i == 3)
                    {
                        if (screensize[0] > (temp - k))
                        {
                            d2 = temp;
                        }
                    }
                    else
                    {
                        d2 = temp;
                    }
                }
            }


            if(d1 > 8190)
            {
                d1 = -1;
            }
            if(d2 > 8190)
            {
                d2 = -1;
            }

            if((d1 != -1)&&(d2 != -1))
            {
                double x = (Math.Pow((double)d2, 2) - Math.Pow((double)d1, 2) - Math.Pow((double)k, 2)) / (-2 * k);
                double y = Math.Sqrt((double)(d1 ^ 2) - Math.Pow(x, 2));

                //해상도와 스크린 크기를 고려한 좌표
                x = (x / screensize[0]) * resolution[0];
                y = resolution[1] - (y / screensize[1]) * resolution[1];
            }
            

            CalculateTouch((int)x,(int)y);
        }
    }

    
}
