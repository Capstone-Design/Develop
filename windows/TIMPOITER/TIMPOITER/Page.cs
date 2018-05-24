using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace TIMPOITER
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "TIMPOINTER 메뉴";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Bluetooth & service", ClassType=typeof(Scenario3_bluetoothAdvertisement) },
            new Scenario() { Title="Settting", ClassType=typeof(Scenario1_screensetting) },
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
