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
using System.Collections;
using Windows.UI.Input.Preview.Injection;
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
        
        private Guid TimpointerServiceUUID = BluetoothUuidHelper.FromShortId(0xffe0);
        private Guid TimpointerSerialCharacteristicUUID = BluetoothUuidHelper.FromShortId(0xffe1);

        // For touch
        private long[] dataChecked = new long[2];
        private bool inRange = false;
        private Point prePoint = new Point(0, 0);
        private double[,] sensorsMax = new double[2, 4];
        private double screenW = 345;
        private double screenH;
        private long preInputTime;
        private bool autoDetached = false;

        // For ValueChangeHandler
        // L : 0, R : 1
        string[] jsonStr = new string[2];
        string[] stringSeparators = new string[] { "\n" };

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
            SetSensorsMax();
            //DrawDot(10, 10);
        }

        private void SetSensorsMax()
        {
            screenH = screenW * h / w;
            sensorsMax[0, 0] = screenH;
            sensorsMax[0, 1] = screenH * Math.Cos(50 * Math.PI / 180);
            sensorsMax[0, 2] = Math.Sqrt(screenH * screenH + screenW * screenW);
            sensorsMax[0, 3] = screenW * Math.Cos(25 * Math.PI / 180);

            for(int i = 0; i < 4; i++)
            {
                sensorsMax[1, i] = sensorsMax[0, 3 - i];
            }
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
            // BLE연결을 async로 동시에 진행하면 하나면 정상연결되어 연결 시도 관리 스레드 생성.
            Task t = Task.Factory.StartNew(() => {
                BLEConnecter();
            });
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            System.Diagnostics.Debug.WriteLine("Watcher 중지");

            if(characteristics[0] != null)
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
                        ShowStr(string.Format("0x{0:X}", eventArgs.BluetoothAddress) + " Found");
                        connectableDevice.Add(new KeyValuePair<int, BluetoothLEAdvertisementReceivedEventArgs>(index, eventArgs));
                    }
                }
            }
        }

        private void BLEConnecter()
        {
            while (!(deviceConnected[0] == 2 && deviceConnected[1] == 2))
            {
                if(connectableDevice.Count > 0)
                {
                    if (deviceConnected[0] != 1 && deviceConnected[1] != 1)
                    {
                        if(deviceConnected[connectableDevice.ElementAt(0).Key] != 2)
                        {
                            ConnectBLEDevice(connectableDevice.ElementAt(0).Value, connectableDevice.ElementAt(0).Key);
                        }
                        connectableDevice.RemoveAt(0);
                    }
                }
                Task.Delay(100);
            }
            watcher.Stop();
            if (deviceConnected[0] == 2 && deviceConnected[1] == 2)
            {
                characteristics[1].ValueChanged += ValueChangeHandlerR;
                characteristics[0].ValueChanged += ValueChangeHandlerL;
                
                //SendData(characteristics[0], "OK");
                //SendData(characteristics[1], "OK");
                ShowStr("연결완료");
                Task t = Task.Factory.StartNew(() => {
                    ConsumeTouch();
                });
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
                            ShowStr("Notification resigter " + result + device.Name);
                            if (result == GattCommunicationStatus.Success)
                            {
                                // 처음 연결시 데이터를 보내 아두이노에서 시리얼 개통을함.
                                //SendData(characteristics[index], " ");
                                deviceConnected[index] = 2;
                                //characteristics[index].ValueChanged += ValueChangeHandlerL;
                                return;
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
                    if(root.Count == 4)
                    {
                        leftData.Add(root);
                        DrawDetect(0, root);
                    }
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
                    if (root.Count == 4)
                    {
                        rightData.Add(root);
                        DrawDetect(1, root);
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

        private void ConsumeTouch()
        {
            while (true)
            {
                if (currentTimeMillis() - preInputTime > 100 && autoDetached == false)
                {
                    autoDetached = true;
                    inRange = false;
                    Touch(2, prePoint.X, prePoint.Y);
                }

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
                            double leftDistance = Calib(left, 0);
                            double rightDistance = Calib(right, 1);
                            Point point;
                            if (leftDistance >= 0 && rightDistance >= 0)
                            {
                                point = CalcXY(leftDistance, rightDistance);
                            }
                            else
                            {
                                continue;
                            }
                            Debug.WriteLine(point);
                            if (point.X >= 0 && point.X <= w && point.Y > 0 && point.Y <= h)
                            {
                                
                                if (inRange)
                                {
                                    Touch(1, point.X, point.Y);
                                }
                                else
                                {
                                    inRange = true;
                                    Touch(0, point.X, point.Y);
                                }
                                prePoint = point;
                                preInputTime = currentTimeMillis();
                                autoDetached = false;
                            }
                            else if(inRange)
                            {
                                autoDetached = false;
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
                        Task.Delay(20);
                    }
                }
                else
                {
                    rightData.Clear();
                    Task.Delay(20);
                    //dataChecked[0];
                }
            }
        }
        private int w = 1920;
        private int h = 1080;
        private Point CalcXY(double l, double r)
        {
            double x = (w*w + l*l - r*r) / (2 * w);
            double y = Math.Sqrt((Math.Abs(l*l - x*x)));
            y = h - y;

            //ShowStr("" + x + " " + y);
            return new Point(x, y);
        }

        
        private double Calib(JsonArray array, int deviceIndex)
        {
            double shortest = 8000/screenW*w;
            for(int i = 0; i < array.Count; i++)
            {
                double raw = array[i].GetNumber();
                raw = raw - 45;
                if (raw > 0 && raw < sensorsMax[deviceIndex, i])
                {
                    raw = raw / screenW * w;
                    if (raw < shortest)
                    {
                        shortest = raw;
                    }
                }
            }
            if (shortest < 8000)
            {
                // TODO 가로, 세로 비율이 다를 수 있음.
                return shortest;
            }
            else
            {
                return -1;
            }

        }

        private static InputInjector inputInjector;
        private InjectedInputRectangle touchContact = new InjectedInputRectangle
        {
            Top = 50,
            Bottom = 50,
            Left = 40,
            Right = 40
        };

        private int Touch(int type, double X, double Y)
        {
            if(inputInjector == null)
            {
                inputInjector = InputInjector.TryCreate();
                // Default는 기본 터치 이펙트, Indirect는 원에 +표시가 있는 마크가 표시됨.
                inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Default);
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
            //DrawDot(X, Y);
            try
            {
                inputInjector.InjectTouchInput
                (
                    new List<InjectedInputTouchInfo>
                    {
                        new InjectedInputTouchInfo
                        {
                            Contact = touchContact,
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
        private List<Ellipse> dotList = new List<Ellipse>();
        private async void DrawDot(double x, double y)
        {
            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                int dotSize = 10;

                Ellipse currentDot = new Ellipse
                {
                    Stroke = new SolidColorBrush(Windows.UI.Colors.SteelBlue),
                    StrokeThickness = 3
                };
                Canvas.SetZIndex(currentDot, 3);
                currentDot.Height = dotSize;
                currentDot.Width = dotSize;
                currentDot.Fill = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
                currentDot.Margin = new Thickness(x, y, 0, 0); // Sets the position.
                dotList.Add(currentDot);
                myCanvas.Children.Add(currentDot);
                if(dotList.Count > 10)
                {
                    for(int i = 0; i < dotList.Count-10; i++)
                    {
                        myCanvas.Children.Remove(dotList[i]);
                        dotList.RemoveAt(i);
                    }
                }
                
            });
        }

        private Line[,] preline = new Line[2,3];
        private async void DrawDetect(int direction, JsonArray array)
        {
            double distance = 9999;
            int index = -1;
            for(int i = 0; i < 4; i++)
            {
                double raw = array[i].GetNumber();
                raw = raw - 45;
                if (raw > 0 && raw < sensorsMax[direction, i])
                {
                    raw = raw / screenW * w;
                    if (raw < distance)
                    {
                        distance = raw;
                        index = i;
                    }
                }
            }
            if (index == -1) return;
            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if(preline[direction, 0] != null)
                {
                    myCanvas.Children.Remove(preline[direction, 0]);
                    myCanvas.Children.Remove(preline[direction, 1]);
                    myCanvas.Children.Remove(preline[direction, 2]);
                }
                
                var line1 = new Line();
                line1.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
                var line2 = new Line();
                line2.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
                var line3 = new Line();
                line3.Stroke = new SolidColorBrush(Windows.UI.Colors.Black);
                double dgree;

                if (direction == 0)
                {
                    line1.X1 = 0;
                    line1.Y1 = h;
                    dgree = index * 25 * Math.PI / 180;
                    //line1.X2 = distance * Math.Cos(dgree);
                    line1.X2 = distance * Math.Cos(dgree);
                    line1.Y2 = h - distance * Math.Sin(dgree);

                    dgree = dgree + 25 * Math.PI / 180;
                    double x = line1.X2;
                    double y = line1.Y2;
                    line2.X1 = x;
                    line2.Y1 = y;

                    line2.X2 = x * Math.Cos(dgree) - (h - y) * Math.Sin(dgree);
                    line2.Y2 = h - x * Math.Sin(dgree) + (h - y) * Math.Cos(dgree);
                }
                else
                {
                    line1.X1 = w;
                    line1.Y1 = h;
                    dgree = index * 25 * Math.PI / 180;
                    line1.X2 = w - distance * Math.Cos(dgree);
                    line1.Y2 = h - distance * Math.Sin(dgree);

                    dgree = dgree + 25 * Math.PI / 180;
                    double x = line1.X2;
                    double y = line1.Y2;
                    line2.X1 = x;
                    line2.Y1 = y;

                    line2.X2 = (w - x) * Math.Cos(dgree) - (h - y) * Math.Sin(dgree);
                    line2.Y2 = h - (w - x) * Math.Sin(dgree) + (h - y) * Math.Cos(dgree);
                }
                line3.X1 = line2.X2;
                line3.Y1 = line2.Y2;
                line3.X2 = line1.X1;
                line3.Y2 = line1.Y1;

                preline[direction, 0] = line1;
                preline[direction, 1] = line2;
                preline[direction, 2] = line3;
                myCanvas.Children.Add(line1);
                myCanvas.Children.Add(line2);
                myCanvas.Children.Add(line3);
            });
        }
    }

}
