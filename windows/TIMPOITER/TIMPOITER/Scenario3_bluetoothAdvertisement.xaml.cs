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


namespace TIMPOITER
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    /// 

    public sealed partial class Scenario3_bluetoothAdvertisement : Page
    {
        int first = 0;
        Guid TimpointerServiceUUID = BluetoothUuidHelper.FromShortId(0xdfb0);
        Guid TimpointerSerialCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xdfb1);
        Guid TimpointerATCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xdfb2);
        private BluetoothLEAdvertisementWatcher watcher;
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



            //if (localName.Equals("Bluno"))
            //{
            if (first++ != 0)
            {


                watcher.Stop();
                BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                DeviceInformationCustomPairing customPairing = device.DeviceInformation.Pairing.Custom;
                DevicePairingKinds ceremoniesSelected = DevicePairingKinds.None
                                                        | DevicePairingKinds.ConfirmOnly
                                                        | DevicePairingKinds.DisplayPin
                                                        | DevicePairingKinds.ProvidePin
                                                        | DevicePairingKinds.ConfirmPinMatch
                                                        ;

                //DevicePairingResult devicePairingResult = await customPairing.PairAsync(ceremoniesSelected);
                //DevicePairingResult devicePairingResult = await device.DeviceInformation.Pairing.PairAsync(DevicePairingProtectionLevel.None);
                System.Diagnostics.Debug.WriteLine("Try to Connect");
                //System.Diagnostics.Debug.WriteLine(devicePairingResult.Status);

                //if (!device.DeviceInformation.Pairing.IsPaired)
                //{


                GattDeviceServicesResult serviceResult = await device.GetGattServicesForUuidAsync(TimpointerServiceUUID);
                if (serviceResult.Status == GattCommunicationStatus.Success)
                {
                    GattCharacteristicsResult ATcharacteristicsResult = await serviceResult.Services.ElementAt(0).GetCharacteristicsForUuidAsync(TimpointerATCharacteristicUUID);
                    GattCharacteristic ATCharacteristic = ATcharacteristicsResult.Characteristics.ElementAt(0);
                    String mPassword = "AT+PASSWOR=DFRobot\r\n";
                    String mBaudrateBuffer = "AT+CURRUART=115200\r\n";
                    var dataWriter = new Windows.Storage.Streams.DataWriter();
                    dataWriter.WriteString(mPassword);
                    await ATCharacteristic.WriteValueAsync(dataWriter.DetachBuffer());
                    dataWriter.WriteString(mBaudrateBuffer);
                    await ATCharacteristic.WriteValueAsync(dataWriter.DetachBuffer());

                    GattCharacteristicsResult serialCharsticsResult = await serviceResult.Services.ElementAt(0).GetCharacteristicsForUuidAsync(TimpointerSerialCharacteristicUUID);
                    if (serialCharsticsResult.Status == GattCommunicationStatus.Success)
                    {
                        GattCharacteristic characteristic = serialCharsticsResult.Characteristics.ElementAt(0);
                        System.Diagnostics.Debug.WriteLine(characteristic.Uuid + " is found");

                        //var descriptorResult = await characteristic.GetDescriptorsForUuidAsync(TimpointerSerialCharacteristicUUID);
                        //GattDescriptor descriptor = descriptorResult.Descriptors[0];
                        //byte[] setNotify = { 0x01, 0x00 };
                        //await descriptor.WriteValueAsync(setNotify.AsBuffer());
                        //characteristic.ValueChanged += valueChangeHandler;
                        try
                        {
                            // TODO Notification 설정이 알수 없는 이유로 실패.
                            var result = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                            characteristic.ValueChanged += valueChangeHandler;
                            //   System.Diagnostics.Debug.WriteLine("Notify set " + result.ProtocolError.Value);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine("Notify set error" + e.StackTrace);
                        }
                        byte[] ByteArray = { (byte)'H', (byte)'I' };
                        IBuffer buffer = ByteArray.AsBuffer();
                        await characteristic.WriteValueAsync(buffer);
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
                //}
                //}
            }
        }

        private void valueChangeHandler(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.CharacteristicValue);
        }

        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

            });
        }

    }
}
