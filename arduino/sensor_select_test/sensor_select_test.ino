#include <Wire.h>
// Import VL53L0X Pololu Library v1.0.2
#include <VL53L0X.h>
#include "Sensors.h"
#include "Battery.h"

Sensors manager;
Battery batt;

void setup()
{
  Serial.begin(115200);
  manager = Sensors(&Serial, 2, 9, 10);
  batt = Battery(5010, 3200, 900);
}

void loop()
{
  Serial.println(manager.GetDistance());
  Serial.println(batt.ReadVcc());
}
