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
using Windows.Graphics.Display;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace TIMPOITER
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Scenario1_screensetting : Page
    {
        private MainPage rootPage = MainPage.Current;
        public Scenario1_screensetting()
        {
            this.InitializeComponent();
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(500, 500);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

            SettingValue a = SettingValue.GetInstance();
            TextBox_x.Text = a.GetResolution()[0].ToString();
            TextBox_y.Text = a.GetResolution()[1].ToString();
            screensize_x.Text = a.GetScreenSize()[0].ToString();
            screensize_y.Text = a.GetScreenSize()[1].ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SettingValue a = SettingValue.GetInstance();
            int[] sc = {Convert.ToInt32(screensize_x.Text), Convert.ToInt32(screensize_y.Text)};
            a.SetScreenSize(sc);
            ToastHelper.ShowToast("화면크기 적용완료");
        }
    }
}
