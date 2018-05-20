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
using System.Threading;
using System.Collections;
using Windows.UI.Input.Preview.Injection;

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
        ulong[] DEVICE_MAC = { 0xD43639C466EA, 0x60640590981E };
        string[] DEVICE_NAME = { "TimpointerL", "TimpointerR" };
        int[] deviceConnected = { 0, 0 };
        private GattCharacteristic[] characteristics = new GattCharacteristic[2];
        List<JsonArray> leftData = new List<JsonArray>();
        List<JsonArray> rightData = new List<JsonArray>();

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
            if (deviceConnected[0] > 0 && deviceConnected[1] > 0)
            {
                if (deviceConnected[0] == 2 && deviceConnected[1] == 2)
                {
                    watcher.Stop();
                }
                return;
            }

            int index;
            for (index = 0; index < DEVICE_MAC.Length; index++)
            {
                if (DEVICE_MAC[index].Equals(eventArgs.BluetoothAddress))
                {
                    if (deviceConnected[index] == 0)
                    {
                        ShowStr(string.Format("0x{0:X}", eventArgs.BluetoothAddress) + " Found");
                        deviceConnected[index] = 1;
                        ConnectBLEDevice(eventArgs, index);
                    }
                }
            }
        }

        private async void ConnectBLEDevice(BluetoothLEAdvertisementReceivedEventArgs eventArgs, int i)
        {
            int index = i;
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
                            ShowStr("Notification resigter " + result + device.Name);
                            if (result == GattCommunicationStatus.Success)
                            {
                                // 처음 연결시 데이터를 보내 아두이노에서 시리얼 개통을함.
                                //SendData(characteristics[index], " ");
                                deviceConnected[index] = 2;
                                if (deviceConnected[0] == 2 && deviceConnected[1] == 2)
                                {
                                    characteristics[0].ValueChanged += ValueChangeHandlerL;
                                    characteristics[1].ValueChanged += ValueChangeHandlerR;
                                    SendData(characteristics[0], "OK");
                                    SendData(characteristics[1], "OK");
                                    ShowStr("두 장치 연결됨");
                                    Thread th = new Thread(new ThreadStart(ConsumeTouch));
                                    th.Start();
                                }
                                return;
                                //ConnectionCheck();
                            }
                        }
                        catch (Exception e)
                        {
                            ShowStr("Notify set error" + e.StackTrace);
                        }
                    }
                    else
                    {
                        ShowStr("Find charateristic error" + serialCharsticsResult.Status);
                    }
                }
                else
                {
                    ShowStr("Find service error" + serviceResult.Status);
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
        // 값 읽기 전용 변수
        // L : 0, R : 1
        string[] jsonStr = new string[2];
        string[] stringSeparators = new string[] { "\n" };

        // TODO 좌우를 위해 함수 혹은 json을 분리완성하도록.
        private void ValueChangeHandlerL(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            int index = 0;
            string str = "";
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[args.CharacteristicValue.Capacity];
            reader.ReadBytes(input);
            reader.DetachBuffer();
            str = System.Text.Encoding.UTF8.GetString(input);
            if (str.Contains("\n"))
            {
                //Debug.WriteLine(str);
                string[] splitted = str.Split(stringSeparators, StringSplitOptions.None);

                string toParse = jsonStr[index];
                if (!splitted[0].Equals(""))
                {
                    // "\n"으로 스트링이 시작하면, index 0이 ""이므로 생략.
                    toParse += splitted[0].Substring(0, splitted[0].Length);
                }
                Debug.WriteLine("L : " + toParse);
                // 초기 시작시 JSON 앞부분을 받지 못하는경우가 있음.
                try
                {
                    JsonArray root = JsonValue.Parse(toParse).GetArray();
                    leftData.Add(root);
                }
                catch (Exception e)
                {
                    //Debug.WriteLine("L Parse error" + e.StackTrace);
                    // Do Nothing
                }
                jsonStr[index] = splitted[1];
            }
            else
            {
                jsonStr[index] += str;
            }

        }

        private void ValueChangeHandlerR(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            int index = 1;
            string str = "";
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[args.CharacteristicValue.Capacity];
            reader.ReadBytes(input);
            reader.DetachBuffer();
            str = System.Text.Encoding.UTF8.GetString(input);
            if (str.Contains("\n"))
            {
                //Debug.WriteLine(str);
                string[] splitted = str.Split(stringSeparators, StringSplitOptions.None);

                string toParse = jsonStr[index];
                if (!splitted[0].Equals(""))
                {
                    // "\n"으로 스트링이 시작하면, index 0이 ""이므로 생략.
                    toParse += splitted[0].Substring(0, splitted[0].Length);
                }
                Debug.WriteLine("R : " + toParse);

                // 초기 시작시 JSON 앞부분을 받지 못하는경우가 있음.
                try
                {
                    JsonArray root = JsonValue.Parse(toParse).GetArray();
                    rightData.Add(root);
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

        private async void ShowStr(string str)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ReceivedAdvertisementListBox.Items.Add(str);
            });
            Debug.WriteLine(str);
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

        private long[] dataChecked = new long[2];
        private bool inRange = false;
        private Point prePoint = new Point(0,0);
        private void ConsumeTouch()
        {
            while (true)
            {
                if (leftData.Count > 0)
                {
                    if (rightData.Count > 0)
                    {
                        try
                        {

                            JsonArray left = leftData.First();
                            leftData.RemoveAt(0);
                            JsonArray right = rightData.First();
                            rightData.RemoveAt(0);

                            int leftDistance = FindValue(left);
                            int rightDistance = FindValue(right);
                            if (leftDistance < 8000 && leftDistance > -1 && rightDistance < 8000 && rightDistance > -1)
                            {
                                Point point = CalcXY(leftDistance, rightDistance);
                                prePoint = point;
                                if (inRange)
                                {
                                    Touch(2, point.X, point.Y);
                                }
                                else
                                {
                                    inRange = true;
                                    Touch(1, point.X, point.Y);
                                }

                            }
                            else
                            {
                                inRange = false;
                                Touch(2, prePoint.X, prePoint.Y);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        leftData.Clear();
                        //dataChecked[0];
                    }
                }
                else
                {
                    rightData.Clear();
                    //dataChecked[0];
                }
            }
        }
        private double screenW = 345;
        private Point CalcXY(int left, int right)
        {
            //SettingValue a = SettingValue.GetInstance();
            int w = 1920;
            int h= 1080;
            double calibX = w / screenW;
            double l = (left - 45) * calibX;
            double r = (right - 45) * calibX;
            double x = (w*w + l*l - r*r) / (2 * w);
            double y = Math.Sqrt((h*h - x*x));

            ShowStr("" + x + " " + y);
            return new Point(x, y);
        }

        private int FindValue(JsonArray json)
        {
            int shortest = 8000;
            for(int i = 0; i < json.Count; i++)
            {
                if(shortest > json[i].GetNumber())
                {
                    shortest = (int)json[i].GetNumber();
                }
            }
            return shortest;
        }
        private static InputInjector inputInjector;
        private int Touch(int type, double X, double Y)
        {
            if(inputInjector == null)
            {
                inputInjector = InputInjector.TryCreate();
                // Default는 기본 터치 이펙트, Indirect는 원에 +표시가 있는 마크가 표시됨.
                inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Indirect);
            }
            InjectedInputPointerOptions pointerOption;
            Double pressure;        // 0.0~ 1.0 (1024단계)
            switch (type)
            {
                case 0:
                    pointerOption = InjectedInputPointerOptions.InRange;
                    pressure = 1.0;
                    break;
                case 1:
                    pointerOption = InjectedInputPointerOptions.InContact;
                    pressure = 1.0;
                    break;
                case 2:
                    pointerOption = InjectedInputPointerOptions.PointerUp;
                    pressure = 0;
                    break;
                default:
                    return -1;
            }
            var location = new InjectedInputPoint { PositionX = (int)X, PositionY = (int)Y };

            try
            {
                inputInjector.InjectTouchInput
                (
                    new List<InjectedInputTouchInfo>
                    {
                        new InjectedInputTouchInfo
                        {
                            Contact = new InjectedInputRectangle
                            {
                                Top = 50,
                                Bottom = 50,
                                Left = 40,
                                Right = 40
                            },
                            PointerInfo = new InjectedInputPointerInfo
                            {
                                PixelLocation = location,
                                PointerOptions = pointerOption,
                                PointerId = 1,
                            },
                            Pressure = pressure,
                            TouchParameters =
                            InjectedInputTouchParameters.Pressure |
                            InjectedInputTouchParameters.Contact
                        }
                    }
                );
                //ShowStr("Touch at : "+ X + " " + Y);
                return 1;
            }
            catch (Exception e)
            {
                return -2;
            }
        }
    }

}
