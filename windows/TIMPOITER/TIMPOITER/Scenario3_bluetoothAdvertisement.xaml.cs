using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

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
     * AT Command : http://blog.naver.com/PostView.nhn?blogId=xisaturn&logNo=220712028679
     * 
     */
        // Connect to HM-10 Default Ble
        // index 0 = left, 1 = right
        private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private long currentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        private ulong[] DEVICE_MAC = { 0xD43639C466EA, 0x60640590981E };
        private string[] DEVICE_NAME = { "TimpointerL", "TimpointerR" };
        private int[] deviceConnected = { 0, 0 };
        private GattCharacteristic[] characteristics = new GattCharacteristic[2];
        private List<JsonArray> leftData = new List<JsonArray>();
        private List<JsonArray> rightData = new List<JsonArray>();
        private List<KeyValuePair<int, BluetoothLEAdvertisementReceivedEventArgs>> connectableDevice = new List<KeyValuePair<int, BluetoothLEAdvertisementReceivedEventArgs>>();
        private bool isScanning = false;
        private Guid TimpointerServiceUUID = BluetoothUuidHelper.FromShortId(0xffe0);
        private Guid TimpointerSerialCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xffe1);
        private BluetoothLEAdvertisementReceivedEventArgs[] connectedDeviceInfo = new BluetoothLEAdvertisementReceivedEventArgs[2];
        private long scanTime = 0;
        private long leftTime = 0;
        private long rightTime = 0;

        // For ValueChangeHandler
        // L : 0, R : 1
        private string[] jsonStr = new string[2];
        private string[] stringSeparators = new string[] { "\n" };

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
            isScanning = true;
            scanTime = currentTimeMillis();
            // BLE연결을 async로 동시에 진행하면 하나면 정상연결되어 연결 시도 관리 스레드 생성.
            Task t = Task.Factory.StartNew(() => {
                BLEConnecter();
            });
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            isScanning = false;
            System.Diagnostics.Debug.WriteLine("Watcher 중지");

            if (characteristics[0] != null)
                characteristics[0].ValueChanged -= ValueChangeHandlerL;
            if (characteristics[1] != null)
                characteristics[1].ValueChanged -= ValueChangeHandlerR;
            for (int i = 0; i < 2; i++)
            {
                deviceConnected[i] = 0;

                characteristics[i] = null;
            }

            leftData.Clear();
            rightData.Clear();
            connectableDevice.Clear();
        }


        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            
            int index;
            for (index = 0; index < DEVICE_MAC.Length; index++)
            {
                if (DEVICE_MAC[index].Equals(eventArgs.BluetoothAddress))
                {
                    if (deviceConnected[index] == 0)
                    {
                        connectableDevice.Add(new KeyValuePair<int, BluetoothLEAdvertisementReceivedEventArgs>(index, eventArgs));
                    }
                }
            }
        }

        private void BLEConnecter()
        {
            while (!(deviceConnected[0] == 2 && deviceConnected[1] == 2) && isScanning == true)
            {
                if (connectableDevice.Count > 0)
                {
                    if (deviceConnected[0] != 1 && deviceConnected[1] != 1)
                    {
                        if (deviceConnected[connectableDevice.ElementAt(0).Key] != 2)
                        {
                            ConnectBLEDevice(connectableDevice.ElementAt(0).Value, connectableDevice.ElementAt(0).Key);
                        }
                        connectableDevice.RemoveAt(0);
                    }
                }
                else
                {
                    if(currentTimeMillis() - scanTime > 10000)
                    {
                        if (deviceConnected[0] == 0 && deviceConnected[1] == 0)
                        {
                            ToastHelper.ShowToast("센서를 찾을 수 없습니다. 다시 연결해주세요");
                            watcher.Stop();
                            return;
                        }
                        else if (deviceConnected[0] == 0)
                        {
                            ToastHelper.ShowToast("왼쪽센서를 찾을 수 없습니다. 센서를 확인해주세요");
                            watcher.Stop();
                            return;
                        }
                        else if (deviceConnected[0] == 0)
                        {
                            ToastHelper.ShowToast("오른쪽센서를 찾을 수 없습니다. 센서를 확인해주세요");
                            watcher.Stop();
                            return;
                        }
                    }
                }
                Task.Delay(100);
            }
            isScanning = false;
            watcher.Stop();
            if (deviceConnected[0] == 2 && deviceConnected[1] == 2)
            {
                characteristics[1].ValueChanged += ValueChangeHandlerR;
                characteristics[0].ValueChanged += ValueChangeHandlerL;

                ToastHelper.ShowToast("모듈 연결 완료");
            
                Task t = Task.Factory.StartNew(() => {
                    Touch.GetInstance().ConsumeTouch(ref leftData, ref rightData, ref isScanning);
                });
            }
            while (true)
            {
                long current = currentTimeMillis();
                if (current - leftTime > 1000 && deviceConnected[0] != 1)
                {
                    ConnectBLEDevice(connectedDeviceInfo[0], 0);
                    ToastHelper.ShowToast("왼쪽센서와 연결이 끊겼습니다. 재연결을 시도합니다.");
                }
                if (current - rightTime > 1000 && deviceConnected[1] != 1)
                {
                    ConnectBLEDevice(connectedDeviceInfo[1], 1);
                    ToastHelper.ShowToast("오른쪽센서와 연결이 끊겼습니다. 재연결을 시도합니다.");
                }
                if(current - leftTime > 5000)
                {
                    ToastHelper.ShowToast("왼쪽센서와 연결되지 않습니다. 센서를 확인해주세요");
                    return;
                }
                if (current - rightTime > 5000)
                {
                    ToastHelper.ShowToast("오른쪽센서와 연결되지 않습니다. 센서를 확인해주세요");
                    return;
                }
            }
        }

        private async void ConnectBLEDevice(BluetoothLEAdvertisementReceivedEventArgs eventArgs, int i)
        {
            int index = i;
            deviceConnected[index] = 1;
            try
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                GattDeviceServicesResult serviceResult = await device.GetGattServicesForUuidAsync(TimpointerServiceUUID);
                if (serviceResult.Status == GattCommunicationStatus.Success)
                {
                    GattCharacteristicsResult serialCharsticsResult = await serviceResult.Services.ElementAt(0).GetCharacteristicsForUuidAsync(TimpointerSerialCharacteristicUUID);
                    if (serialCharsticsResult.Status == GattCommunicationStatus.Success)
                    {
                        characteristics[index] = serialCharsticsResult.Characteristics.ElementAt(0);
                        try
                        {
                            var result = await characteristics[index].WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            //ShowStr("Notification resigter " + result + device.Name);
                            if (result == GattCommunicationStatus.Success)
                            {
                                // 처음 연결시 데이터를 보내 아두이노에서 시리얼 개통을함.
                                deviceConnected[index] = 2;
                                //characteristics[index].Service.Session.MaintainConnection = true;
                                connectedDeviceInfo[index] = eventArgs;
                                //Debug.WriteLine(characteristics[index].Service.Session.SessionStatus);
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            //ShowStr("Notify set error" + e.StackTrace);
                        }
                    }
                    else
                    {
                        //ShowStr("Find charateristic error" + serialCharsticsResult.Status);
                    }
                }
                else
                {
                    //ShowStr("Find service error" + serviceResult.Status);
                }
            }
            catch (Exception e)
            {

            }
            // 재연결 시도
            deviceConnected[index] = 0;
            if (watcher.Status == BluetoothLEAdvertisementWatcherStatus.Stopped)
            {
                watcher.Start();
            }
        }

        private void ValueChangeHandlerL(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            leftTime = currentTimeMillis();
            int index = 0;
            ParseGattValue(index, args);

        }

        private void ValueChangeHandlerR(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            rightTime = currentTimeMillis();
            int index = 1;
            ParseGattValue(index, args);

        }

        private void ParseGattValue(int index, GattValueChangedEventArgs args)
        {
            string str = "";
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[args.CharacteristicValue.Capacity];
            reader.ReadBytes(input);
            reader.DetachBuffer();
            str = System.Text.Encoding.UTF8.GetString(input);
            if (str.Contains("\n"))
            {
                string[] splitted = str.Split(stringSeparators, StringSplitOptions.None);

                string toParse = jsonStr[index];
                if (!splitted[0].Equals(""))
                {
                    // "\n"으로 스트링이 시작하면, index 0이 ""이므로 생략.
                    toParse += splitted[0].Substring(0, splitted[0].Length);
                }

                // 초기 시작시 JSON 앞부분을 받지 못하는경우가 있음.
                try
                {
                    if (toParse.Contains("batt"))
                    {
                        string[] batt = toParse.Split(":");
                        SettingValue.GetInstance().SetBattery(index, int.Parse(batt[1]));
                    }
                    else
                    {
                        JsonArray root = JsonValue.Parse(toParse).GetArray();
                        if (root.Count == 4)
                        {
                            List<JsonArray> array;
                            if (index == 0)
                            {
                                array = leftData;
                            }
                            else
                            {
                                array = rightData;
                            }

                            array.Add(root);
                        }
                    }

                }
                catch (Exception e)
                {
                    //Debug.WriteLine("R Parse error" + e.StackTrace);
                }
                jsonStr[index] = splitted[1];
            }
            else
            {
                jsonStr[index] += str;
            }
        }

        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

            });
        }

      
    }

}
