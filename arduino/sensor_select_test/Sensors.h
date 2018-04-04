#ifndef Sensors_h
#define Sensors_h

#include "Arduino.h"
#include <VL53L0X.h>

// Usage : Init(Sensors count, Sensors xshut pins...) then call int GetDistance()

class Sensors{
  public:
    // count : Number of sensor, xshut pins,
    // Init sensors, set adress, start sensing.
    void Init(int count, ...);

    // print to serial with data format.
    void SendDistance(HardwareSerial &mySerial, String id, String distance);

    // return : average of distances from normally measured.
    int GetDistance();
  private:
    void DisableAllSensor();
    void Sensor::SetSensor(int num);
    VL53L0X *sensors;
    int *sensorXshutPins;
    int sensorCount;
}
#endif
