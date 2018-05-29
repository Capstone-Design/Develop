using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;  
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;


namespace TIMPOITER
{
    /// <summary>
    /// 기본 응용 프로그램 클래스를 보완하는 응용 프로그램별 동작을 제공합니다.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Singleton 응용 프로그램 개체를 초기화합니다. 이것은 실행되는 작성 코드의 첫 번째
        /// 줄이며 따라서 main() 또는 WinMain()과 논리적으로 동일합니다.
        /// </summary>
        /// 
        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static bool IsForeground = false;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            //트레이 아이콘을 위한 코드 
            this.LeavingBackground += App_LeavingBackground;
            this.EnteredBackground += App_EnteredBackground;
            
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(700, 500);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;
            
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            IsForeground = false;
        }

        private void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            IsForeground = true;
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            // connection established from the fulltrust process
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                AppServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnTaskCanceled;
                Connection = details.AppServiceConnection;
                Connection.RequestReceived += OnRequestReceived;
            }
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (args.Request.Message.ContainsKey("content"))
            {
                object message = null;
                args.Request.Message.TryGetValue("content", out message);
                if (App.IsForeground)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(async () =>
                    {
                        MessageDialog dialog = new MessageDialog(message.ToString());
                        await dialog.ShowAsync();
                    }));
                }
                else
                {
                    ToastHelper.ShowToast(message.ToString());
                }
            }

            if (args.Request.Message.ContainsKey("resolutionchaged"))
            {
                object message = null;
                args.Request.Message.TryGetValue("resolutionchaged", out message);
                int a = Convert.ToInt32(message);
                SettingValue.GetInstance().ResolutionChanged(a / 10000, a % 10000);
                System.Diagnostics.Debug.WriteLine("resolutionchaged");

                ToastHelper.ShowToast("해상도 변경 : " + a / 10000 + " X " + a % 10000);

            }

            if (args.Request.Message.ContainsKey("autoSetting"))
            {
                object message = null;
                args.Request.Message.TryGetValue("autoSetting", out message);
                int a = Convert.ToInt32(message);
                int[] resolution = SettingValue.GetInstance().ResolutionChanged(a / 10000, a % 10000);
                double actualSizeInInches = Double.MaxValue;
                if (DisplayInformation.GetForCurrentView().DiagonalSizeInInches.HasValue)
                    actualSizeInInches = DisplayInformation.GetForCurrentView().DiagonalSizeInInches.Value;
                double d = resolution[0] ^ 2 + resolution[1] ^ 2;
                d = actualSizeInInches / Math.Sqrt(d);
                int[] size = { (int)(d * resolution[0]), (int)(d * resolution[1]) };
                SettingValue.GetInstance().SetScreenSize(size);

                Scenario3_bluetoothAdvertisement.Current.reSetting();
            }

            if (args.Request.Message.ContainsKey("openSetting"))
            {
                if (App.IsForeground)
                {
                    MainPage.Current.ScenarioControl_Settting();
                }
            }

            if (args.Request.Message.ContainsKey("battery"))
            {
                object message = null;
                args.Request.Message.TryGetValue("battery", out message);
                int a = Convert.ToInt32(message);

                ToastHelper.ShowToast("왼쪽 모듈 배터리    : " + a / 10000 + "%\n오른쪽 모듈 배터리 : " + a % 10000 + "%");

            }

            if (args.Request.Message.ContainsKey("exit"))
            {
                App.Current.Exit();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (AppServiceDeferral != null)
            {
                AppServiceDeferral.Complete();
            }
        }

        /// <summary>
        /// 최종 사용자가 응용 프로그램을 정상적으로 시작할 때 호출됩니다. 다른 진입점은
        /// 특정 파일을 여는 등 응용 프로그램을 시작할 때
        /// </summary>
        /// <param name="e">시작 요청 및 프로세스에 대한 정보입니다.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 창에 콘텐츠가 이미 있는 경우 앱 초기화를 반복하지 말고,
            // 창이 활성화되어 있는지 확인하십시오.
            if (rootFrame == null)
            {
                // 탐색 컨텍스트로 사용할 프레임을 만들고 첫 페이지로 이동합니다.
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 이전에 일시 중지된 응용 프로그램에서 상태를 로드합니다.
                }

                // 현재 창에 프레임 넣기
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 탐색 스택이 복원되지 않으면 첫 번째 페이지로 돌아가고
                    // 필요한 정보를 탐색 매개 변수로 전달하여 새 페이지를
                    // 구성합니다.
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 현재 창이 활성 창인지 확인
                Window.Current.Activate();
            }


        }

        /// <summary>
        /// 특정 페이지 탐색에 실패한 경우 호출됨
        /// </summary>
        /// <param name="sender">탐색에 실패한 프레임</param>
        /// <param name="e">탐색 실패에 대한 정보</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 응용 프로그램 실행이 일시 중단된 경우 호출됩니다.  응용 프로그램이 종료될지
        /// 또는 메모리 콘텐츠를 변경하지 않고 다시 시작할지 여부를 결정하지 않은 채
        /// 응용 프로그램 상태가 저장됩니다.
        /// </summary>
        /// <param name="sender">일시 중단 요청의 소스입니다.</param>
        /// <param name="e">일시 중단 요청에 대한 세부 정보입니다.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("stop");

            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 응용 프로그램 상태를 저장하고 백그라운드 작업을 모두 중지합니다.
            if(AppServiceDeferral != null)
            {
                AppServiceDeferral.Complete();
            }
            deferral.Complete();
        }
    }
}
