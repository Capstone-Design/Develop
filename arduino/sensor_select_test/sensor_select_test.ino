#include <Wire.h>
// Import VL53L0X Pololu Library v1.0.2
#include <VL53L0X.h>
#include "Sensors.h"
#include "Battery.h"

Sensors manager;
const int SENSOR_COUNT = 2;
const int SENSOR1_XSHUT = 9;
const int SENSOR2_XSHUT = 10;
Battery batt;

void setup()
{
  Serial.begin(115200);
  manager = Sensors(SENSOR_COUNT, SENSOR1_XSHUT, SENSOR2_XSHUT);
  batt = Battery(5010, 3200, 900);
}

void loop()
{ 
  if (Serial.available()>0) {
       int distance[SENSOR_COUNT];
       for(int i = 0; i < SENSOR_COUNT; i++){
         distance[i] = manager.GetDistance(i);
       }
       manager.SendDistance(&Serial, "1", distance);
       Serial.println(batt.ReadVcc());
    }
}

