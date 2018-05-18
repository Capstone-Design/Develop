
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


namespace BackgroundTask
{
    public sealed class BaseTask : IBackgroundTask
    {

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            

            System.Diagnostics.Debug.WriteLine("back ground task sss");
            ApplicationData.Current.LocalSettings.Values[taskInstance.Task.Name] = "성공";

          
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
