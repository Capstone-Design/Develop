using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage;

namespace TIMPOITER
{


    public class SettingValue
    {
        private static SettingValue instance;

        //로컬데이터에 저장 
        ApplicationDataContainer localSettings = null;
        private static int[] screensize = new int[2];
        private static int[] resolution = new int[2];
        private static double[] calibration = new double[2];


        //arduino = new [2];
        //private int firstStart;

        private SettingValue()
        {
            localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings != null)
            {
                screensize[0] = Convert.ToInt32(localSettings.Values["screensizeW"]);
                screensize[1] = Convert.ToInt32(localSettings.Values["screensizeH"]);
                resolution[0] = Convert.ToInt32(localSettings.Values["resolutionW"]);
                resolution[1] = Convert.ToInt32(localSettings.Values["resolutionH"]);

            }
        }

        public static SettingValue GetInstance()
        {
            if (instance == null)
            {
                instance = new SettingValue();
            }
            return instance;
        }

        //해상도가 변경되었을 때의 설정 변경
        public void ResolutionChanged(int x, int y)
        {
            resolution[0] = x;
            resolution[1] = y;
            localSettings.Values["resolutionW"] = resolution[0];
            localSettings.Values["resolutionH"] = resolution[1];
        }

        //디스플레이 사이즈 저장 
        public void SetScreenSize(int[] size)
        {
            screensize[0] = size[0];
            screensize[1] = size[1];
            localSettings.Values["screensizeW"] = screensize[0];
            localSettings.Values["screensizeH"] = screensize[1];
        }

        public int[] GetScreenSize()
        {
            return screensize;
        }

        // 해상도 가져오기 및 저장 
        public int[] GetResolution()
        {
            //resolution[0] = (int)DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
            //resolution[1] = (int)DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;
            resolution[0] = 1920;
            resolution[1] = 1080;
            localSettings.Values["resolutionW"] = resolution[0];
            localSettings.Values["resolutionH"] = resolution[1];

            return resolution;
        }

        /*public int* GetBattery()
        {
            return &; 
        }*/
    }


}
