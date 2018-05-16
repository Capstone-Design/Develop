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
using Windows.Data.Json;
using System.Diagnostics;
using System.Threading.Tasks;


namespace TIMPOITER
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    /// 

    public sealed partial class Scenario3_bluetoothAdvertisement : Page
    {
        /* For Debugging
     * 
     * 이름바꾸고 재부팅 많이해야함.
     * Left MAC : C8FD198307AF
     * Left Name : TimpointerL
     * 
     * Right MAC : D43639C466EA
     * Right Name : TimpointerR
     * 
     * AT Command : http://blog.naver.com/PostView.nhn?blogId=xisaturn&logNo=220712028679
     * 
     */
        // Connect to HM-10 Default Ble
        // index 0 = left, 1 = right
        ulong[] DEVICE_MAC = { 0xC8FD198307AF, 0xD43639C466EA };
        string[] DEVICE_NAME = { "TimpointerL", "TimpointerR" };
        bool left_connected = false;
        bool right_connected = false;
        private GattCharacteristic[] characteristic = new GattCharacteristic[2];

        Guid TimpointerServiceUUID = BluetoothUuidHelper.FromShortId(0xffe0);
        Guid TimpointerSerialCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xffe1);
        
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

            // SyncTask for async
            // 비동기로하면, 연결하는동안 같은 기기가 계속 탐지되여 연결시도됨.
            // TODO Block으로 하지말고, 장치가 연결중인지 표시해 비동기로 장치 두개를 동시에 연결 할수 있도록 시도.
            Task<BluetoothLEDevice> dev = Task.Run(async () =>
            {
                BluetoothLEDevice de = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                return de;
            });
            
            // Block until task returned;
            BluetoothLEDevice device = dev.Result;
            string deviceName = device.Name;
            int index;
            for (index = 0; index < DEVICE_NAME.Length; index++)
            {

                if (DEVICE_NAME[index].Equals(deviceName))
                {
                    // for debug a device
                    watcher.Stop();
                    break;
                }
            }
            if (index == DEVICE_NAME.Length - 1) return;
            ConnectBLEDevice(eventArgs, index);
        }

        private async void ConnectBLEDevice(BluetoothLEAdvertisementReceivedEventArgs eventArgs, int index)
        {
            BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
            GattDeviceServicesResult serviceResult = await device.GetGattServicesForUuidAsync(TimpointerServiceUUID);
            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                GattCharacteristicsResult serialCharsticsResult = await serviceResult.Services.ElementAt(0).GetCharacteristicsForUuidAsync(TimpointerSerialCharacteristicUUID);
                if (serialCharsticsResult.Status == GattCommunicationStatus.Success)
                {
                    characteristic[index] = serialCharsticsResult.Characteristics.ElementAt(0);
                    try
                    {
                        var result = await characteristic[index].WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        characteristic[index].ValueChanged += valueChangeHandler;
                        ShowStr("Notification resigter " + result);
                        if (result == GattCommunicationStatus.Success)
                        {
                            // 처음 연결시 데이터를 보내 아두이노에서 시리얼 개통을함.
                            SendData(characteristic[index], " ");
                            switch (index)
                            {
                                case 0:
                                    left_connected = true;
                                    break;
                                case 1:
                                    right_connected = true;
                                    break;
                            }

                            // 양쪽 장치가 모두 연결되면 Watcher 중지.
                            if ((left_connected && right_connected) == true)
                            {
                                watcher.Stop();
                            }
                        } else
                        {
                            ConnectBLEDevice(eventArgs, index);
                        }
                    }
                    catch (Exception e)
                    {
                        ShowStr("Notify set error" + e.StackTrace);
                        ConnectBLEDevice(eventArgs, index);
                        //System.Diagnostics.Debug.WriteLine("Notify set error" + e.StackTrace);
                    }
                }
                else
                {
                    ShowStr("Find charateristic error" + serialCharsticsResult.Status);
                    ConnectBLEDevice(eventArgs, index);
                }
            }
            else
            {
                ShowStr("Find service error" + serviceResult.Status);
                ConnectBLEDevice(eventArgs, index);
            }
        }

        // 값 읽기 전용 변수
        string jsonStr = "";
        string[] stringSeparators = new string[] { "\n" };

        // TODO 좌우를 위해 함수 혹은 json을 분리완성하도록.
        private void valueChangeHandler(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            // TODO Handle received sensor value.
            string str = "";
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[args.CharacteristicValue.Length];
            reader.ReadBytes(input);
            reader.DetachBuffer();
            str = System.Text.Encoding.UTF8.GetString(input);
            if (str.Contains("\n"))
            {
                Debug.WriteLine(str);
                // 
                string[] splitted = str.Split(stringSeparators, StringSplitOptions.None);
                //ShowStr(jsonStr + splitted[0]);

                // 초기 시작시 JSON 앞부분을 받지 못하는경우가 있음.
                try
                {
                    string toParse = jsonStr;
                    if (!splitted[0].Equals(""))
                    {
                        // "\n"으로 스트링이 시작하면, index 0이 ""이므로 생략.
                        toParse += splitted[0].Substring(0, splitted[0].Length - 1);
                    }
                    Debug.WriteLine(toParse);
                    JsonObject root = JsonValue.Parse(toParse).GetObject();
                    JsonArray distances = root["distance"].GetArray();
                    string toShow = "";
                    for (int i = 0; i < distances.Count; i++)
                    {
                        int value = (int)distances[i].GetNumber();
                        toShow += value + ", ";
                    }
                    //ShowStr(toShow);
                    //Debug.WriteLine(root["id"].GetString());
                    //Debug.WriteLine(root["distance"].GetObject()["State"].GetString());
                }
                catch
                {
                    // Do Nothing
                }
                jsonStr = splitted[1];
            }
            else
            {
                jsonStr += str;
            }

        }

        private async void ShowStr(string str)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ReceivedAdvertisementListBox.Items.Add(str);
            });
        }

        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

            });
        }

        private async void SendData(GattCharacteristic characteristic, string str)
        {
            var dataWriter = new Windows.Storage.Streams.DataWriter();
            dataWriter.WriteString(str);
            var result = await characteristic.WriteValueAsync(dataWriter.DetachBuffer());

        }
    }
}
