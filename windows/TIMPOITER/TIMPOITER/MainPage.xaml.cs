
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.Storage;
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
            ApplicationDataContainer localSettings = null;
            localSettings = ApplicationData.Current.LocalSettings;

            // 첫 실행시  자동 설정 
            if (localSettings == null)
            {
                int[] resolution = SettingValue.GetInstance().GetResolution();
                double actualSizeInInches = Double.MaxValue;
                if (DisplayInformation.GetForCurrentView().DiagonalSizeInInches.HasValue)
                    actualSizeInInches = DisplayInformation.GetForCurrentView().DiagonalSizeInInches.Value;
                double d = resolution[0] ^ 2 + resolution[1] ^ 2;
                d = actualSizeInInches / Math.Sqrt(d);
                int[] a = { (int)(d * resolution[0]), (int)(d * resolution[1])};
                SettingValue.GetInstance().SetScreenSize(a);
            }



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
                //if (Window.Current.Bounds.Width < 640)
                //{
                    Splitter.IsPaneOpen = false;
                //}
            }
        }

        public void ScenarioControl_Settting()
        {
            Scenario s = new Scenario() { Title = "Screen Settting", ClassType = typeof(Scenario1_screensetting) };
            if (s != null)
            {
                ScenarioFrame.Navigate(s.ClassType);
                
                Splitter.IsPaneOpen = false;
               
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

}
