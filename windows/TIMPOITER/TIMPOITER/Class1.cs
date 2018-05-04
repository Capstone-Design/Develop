using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace TIMPOITER
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "TIMPOINTER SETTING";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Screen Settting", ClassType=typeof(Scenario1_screensetting) },
            new Scenario() { Title="Bluetooth Setting", ClassType=typeof(Scenario2_bluetooth) },
            //new Scenario() { Title="Server: Publish foreground", ClassType=typeof(Scenario3_ServerForeground) },
        };

        public string SelectedBleDeviceId;
        public string SelectedBleDeviceName = "No device selected";
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
