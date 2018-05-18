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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace SystrayComponent
{
    class Class1
    {
    }

    public sealed partial class Scenario3_bluetoothAdvertisement : Page
    {
        // Connect to HM-10 Default Ble
        Guid TimpointerServiceUUID = BluetoothUuidHelper.FromShortId(0xffe0);
        Guid TimpointerSerialCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xffe1);
        private BluetoothLEAdvertisementWatcher watcher;
        private GattCharacteristic characteristic;
        private MainPage rootPage;

        public Scenario3_bluetoothAdvertisement()
        {
            this.InitializeComponent();

            // Create and initialize a new watcher instance.
            watcher = new BluetoothLEAdvertisementWatcher();
            watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(TimpointerServiceUUID);
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);
            watcher.ScanningMode = BluetoothLEScanningMode.Active;
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

            Application.Current.Suspending -= App_Suspending;
            Application.Current.Resuming -= App_Resuming;

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
            System.Threading.Tasks.Task.Delay(2000);
            System.Diagnostics.Debug.WriteLine(watcher.Status);
            System.Diagnostics.Debug.WriteLine("Watcher 시작");

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            System.Diagnostics.Debug.WriteLine("Watcher 중지");
        }


        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            System.Diagnostics.Debug.WriteLine("Watcher Received");
            //ToastHelper.ShowToast("Watcher Received");
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

            // TODO filter by Blutooth name, connect with 2 device
            watcher.Stop();
            BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);

            GattDeviceServicesResult serviceResult = await device.GetGattServicesForUuidAsync(TimpointerServiceUUID);
            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                GattCharacteristicsResult serialCharsticsResult = await serviceResult.Services.ElementAt(0).GetCharacteristicsForUuidAsync(TimpointerSerialCharacteristicUUID);
                if (serialCharsticsResult.Status == GattCommunicationStatus.Success)
                {
                    characteristic = serialCharsticsResult.Characteristics.ElementAt(0);
                    try
                    {
                        var result = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        characteristic.ValueChanged += valueChangeHandler;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Notify set error" + e.StackTrace);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(serialCharsticsResult.Status);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(serviceResult.Status);
            }
        }

        private async void valueChangeHandler(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            // TODO Handle received sensor value.
            GattReadResult result = await characteristic.ReadValueAsync();
            var reader = DataReader.FromBuffer(result.Value);
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            string str = System.Text.Encoding.UTF8.GetString(input);
            //string str = Convert.ToString(input);
            //System.Diagnostics.Debug.WriteLine(str);
            System.Diagnostics.Debug.WriteLine(input);
            //ToastHelper.ShowToast(str);
        }

        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendData(characteristic, Data.Text);
            Data.Text = "";
        }

        private async void SendData(GattCharacteristic characteristic, string str)
        {
            var dataWriter = new Windows.Storage.Streams.DataWriter();
            dataWriter.WriteString(str);
            await characteristic.WriteValueAsync(dataWriter.DetachBuffer());
        }
    }
}
