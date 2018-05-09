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
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;


namespace TIMPOITER
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Scenario3_bluetoothAdvertisement : Page
    {
        private BluetoothLEAdvertisementWatcher watcher;

        private MainPage rootPage;
  

        public Scenario3_bluetoothAdvertisement()
        {
            

            this.InitializeComponent();

            // Create and initialize a new watcher instance.
            watcher = new BluetoothLEAdvertisementWatcher();
            Guid TimpointerServiceUUID = BluetoothUuidHelper.FromShortId(0xdfb0);
            //Guid TimpointerCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xdfb1);


            watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(TimpointerServiceUUID);


            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;
            
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;

            
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);

         
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;

            watcher.Received += OnAdvertisementReceived;

            watcher.Stopped += OnAdvertisementWatcherStopped;

            App.Current.Suspending += App_Suspending;
            App.Current.Resuming += App_Resuming;

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
         
            App.Current.Suspending -= App_Suspending;
            App.Current.Resuming -= App_Resuming;

            watcher.Stop();

            watcher.Received -= OnAdvertisementReceived;
            watcher.Stopped -= OnAdvertisementWatcherStopped;

            base.OnNavigatingFrom(e);
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
           
            watcher.Stop();
            
            watcher.Received -= OnAdvertisementReceived;
            watcher.Stopped -= OnAdvertisementWatcherStopped;

        }

   
        private void App_Resuming(object sender, object e)
        {
            watcher.Received += OnAdvertisementReceived;
            watcher.Stopped += OnAdvertisementWatcherStopped;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Start();

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
        }

   
        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            
            DateTimeOffset timestamp = eventArgs.Timestamp;

       
            BluetoothLEAdvertisementType advertisementType = eventArgs.AdvertisementType;

        
            Int16 rssi = eventArgs.RawSignalStrengthInDBm;

       
            string localName = eventArgs.Advertisement.LocalName;

       
            string manufacturerDataString = "";
            var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            if (manufacturerSections.Count > 0)
            {
                var manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);
                }
                manufacturerDataString = string.Format("0x{0}: {1}",
                    manufacturerData.CompanyId.ToString("X"),
                    BitConverter.ToString(data));
            }

         
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ReceivedAdvertisementListBox.Items.Add(string.Format("[{0}]: type={1}, rssi={2}, name={3}, manufacturerData=[{4}]",
                    timestamp.ToString("hh\\:mm\\:ss\\.fff"),
                    advertisementType.ToString(),
                    rssi.ToString(),
                    localName,
                    manufacturerDataString));
            });
        }

        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                
            });
        }

    }
}
