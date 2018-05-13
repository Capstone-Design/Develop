#include <Wire.h>
// Import VL53L0X Pololu Library v1.0.2
#include <VL53L0X.h>
#include "Sensors.h"
#include "Battery.h"
#include <SoftwareSerial.h>

const int SENSOR_COUNT = 4;
const int SENSOR1_XSHUT = 9;
const int SENSOR2_XSHUT = 10;
const int SENSOR3_XSHUT = 11;
const int SENSOR4_XSHUT = 12;

SoftwareSerial hm10(2,3); // HM-10 표기 기준 TX, RX
Sensors manager;
Battery batt;

void setup()
{
  hm10.begin(9600);
  manager = Sensors(SENSOR_COUNT, SENSOR1_XSHUT, SENSOR2_XSHUT, SENSOR3_XSHUT, SENSOR4_XSHUT );
  batt = Battery(5010, 3200, 900);
}

void loop()
{ 
  if (hm10.available()>0) {
       int distance[SENSOR_COUNT];
       for(int i = 0; i < SENSOR_COUNT; i++){
         distance[i] = manager.GetDistance(i);
       }
       
       manager.SendDistance(&hm10, "1", distance);
       // TODO RefreshPercentage를 통해 보내도록 설정.
       hm10.println(batt.ReadVcc());
    }
}

