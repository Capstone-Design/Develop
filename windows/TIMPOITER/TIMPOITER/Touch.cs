using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.Input.Preview.Injection;

namespace TIMPOITER
{

    public class Touch
    {
        private static Touch instance;

        private bool inRange = false;
        private Point prePoint = new Point(0, 0);
        private double[,] sensorsMax = new double[2, 4];
        private double screenW = 600;
        private double screenH;
        private long preInputTime;
        private bool autoDetached = false;

        private int w = 1920;
        private int h = 1080;

        InputInjector inputInjector;

        private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private long currentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        private void SetSensorsMax()
        {
            sensorsMax[0, 0] = screenH;
            sensorsMax[0, 1] = screenH * Math.Cos(50 * Math.PI / 180);
            sensorsMax[0, 2] = Math.Sqrt(screenH * screenH + screenW * screenW);
            sensorsMax[0, 3] = screenW * Math.Cos(25 * Math.PI / 180);

            for (int i = 0; i < 4; i++)
            {
                sensorsMax[1, i] = sensorsMax[0, i];
            }
        }

        public Touch()
        {
            inputInjector = InputInjector.TryCreate();
            inputInjector.InitializePenInjection(InjectedInputVisualizationMode.Default);
           
        }

        public static Touch GetInstance()
        {
            if (instance == null)
            {
                instance = new Touch();
            }

            instance.w = SettingValue.GetInstance().GetResolution()[0];
            instance.h = SettingValue.GetInstance().GetResolution()[1];
            instance.screenW = SettingValue.GetInstance().GetScreenSize()[0];
            instance.screenH = SettingValue.GetInstance().GetScreenSize()[1];
            instance.SetSensorsMax();
            return instance;
        }

        private int TouchInput(int type, double X, double Y)
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


        public void ConsumeTouch(ref List<JsonArray> leftData, ref List<JsonArray> rightData, ref bool isScanning)
        {
            while (true)
            {
                if (isScanning) return;
                if (currentTimeMillis() - preInputTime > 400 && autoDetached == false)
                {
                    autoDetached = true;
                    inRange = false;
                    TouchInput(2, prePoint.X, prePoint.Y);
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
                            //Debug.WriteLine(point);
                            if (point.X >= 0 && point.X <= w && point.Y > 0 && point.Y <= h)
                            {

                                if (inRange)
                                {
                                    TouchInput(1, point.X, point.Y);
                                }
                                else
                                {
                                    inRange = true;
                                    TouchInput(0, point.X, point.Y);
                                }
                                prePoint = point;
                                preInputTime = currentTimeMillis();
                                autoDetached = false;
                            }
                            else if (inRange)
                            {
                                autoDetached = false;
                                inRange = false;
                                TouchInput(2, prePoint.X, prePoint.Y);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        leftData.Clear();
                        Task.Delay(300);
                    }
                }
                else
                {
                    rightData.Clear();
                    Task.Delay(300);
                }
            }
        }

        private Point CalcXY(double l, double r)
        {
            double x = (w * w + l * l - r * r) / (2 * w);
            double y = Math.Sqrt((Math.Abs(l * l - x * x)));
            y = h - y;

            return new Point(x, y);
        }

        private double Calib(JsonArray array, int deviceIndex)
        {
            double shortest = 8000 / screenW * w;
            for (int i = 0; i < array.Count; i++)
            {
                double raw = array[i].GetNumber();
                raw = raw - 45;

                if (raw > 0)
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
    }


}
