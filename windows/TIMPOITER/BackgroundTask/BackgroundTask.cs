
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Preview.Injection;


namespace BackgroundTask
{
    public sealed class BaseTask : IBackgroundTask
    {
        private static InputInjector inputInjector;


        public void Run(IBackgroundTaskInstance taskInstance)
        {
            inputInjector = InputInjector.TryCreate();
            // Default는 기본 터치 이펙트, Indirect는 원에 +표시가 있는 마크가 표시됨.
            inputInjector.InitializeTouchInjection(InjectedInputVisualizationMode.Default);

            System.Diagnostics.Debug.WriteLine("back ground task sss");
            Touch(1, 150, 170);
            System.Threading.Tasks.Task.Delay(100).Wait();
            Touch(2, 150, 170);
            System.Threading.Tasks.Task.Delay(100).Wait();
            Touch(1, 150, 170);
            System.Threading.Tasks.Task.Delay(100).Wait();
            Touch(2, 150, 170);
            ApplicationData.Current.LocalSettings.Values[taskInstance.Task.Name] = "성공";

          
        }


        private int Touch(int type, double X, double Y)
        {
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
                return 1;
            }
            catch (Exception e)
            {
                return -2;
            }
        }

    }






//BLE 통신 
public sealed class BLEScanTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();




            deferral.Complete();
        }
    }
    

    // 값 받기
    public sealed class BLETask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            System.Diagnostics.Debug.WriteLine("WE are @ Background");
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            GattCharacteristicNotificationTriggerDetails details = (GattCharacteristicNotificationTriggerDetails)taskInstance.TriggerDetails;



            var reader = DataReader.FromBuffer(details.Value);
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            reader.DetachBuffer();
            string str = System.Text.Encoding.UTF8.GetString(input);
            System.Diagnostics.Debug.WriteLine(str);
            // TODO send str 

            deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested because " + reason);
        }
    }


    //터치 입력 
    public sealed class TouchTask : IBackgroundTask
    {
        BackgroundTaskDeferral Deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Deferral = taskInstance.GetDeferral();




            Deferral.Complete();
        }
    }


}
