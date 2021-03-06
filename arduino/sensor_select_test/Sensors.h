#ifndef Sensors_h
#define Sensors_h

#include "Arduino.h"
#include <VL53L0X.h>
#include <SoftwareSerial.h>

// Usage : Init(Sensors count, Sensors xshut pins...) then call int GetDistance()
const int DISTANCE_THRESHOLD = 8190;

class Sensors{
  public:
    // count : Number of sensor, xshut pins,
    // Init sensors, set adress, start sensing.
    Sensors();
    Sensors(int count, ...);

    // print to serial with data format.
    void SendDistance(SoftwareSerial *mySerial, String id, int *distance);

    // return : average of distances from normally measured.
    int GetDistance(int sendsorNum);
  private:
    void DisableAllSensor();
    void SetSensor(int num);
    VL53L0X *sensors;
    int *sensorXshutPins;
    int sensorCount;
};
#endif
