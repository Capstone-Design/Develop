#include <Wire.h>
#include <VL53L0X.h>
// Import VL53L0X Pololu Library v1.0.2

VL53L0X Sensor1;
VL53L0X Sensor2;

// Uncomment this line to use long range mode. This
// increases the sensitivity of the sensor and extends its
// potential range, but increases the likelihood of getting
// an inaccurate reading because of reflections from objects
// other than the intended target. It works best in dark
// conditions.

//#define LONG_RANGE


// Uncomment ONE of these two lines to get
// - higher speed at the cost of lower accuracy OR
// - higher accuracy at the cost of lower speed

//#define HIGH_SPEED
//#define HIGH_ACCURACY
  
//#if defined LONG_RANGE
//  // lower the return signal rate limit (default is 0.25 MCPS)
//  sensor.setSignalRateLimit(0.1);
//  // increase laser pulse periods (defaults are 14 and 10 PCLKs)
//  sensor.setVcselPulsePeriod(VL53L0X::VcselPeriodPreRange, 18);
//  sensor.setVcselPulsePeriod(VL53L0X::VcselPeriodFinalRange, 14);
//#endif
//
//#if defined HIGH_SPEED
//  // reduce timing budget to 20 ms (default is about 33 ms)
//  sensor.setMeasurementTimingBudget(20000);
//#elif defined HIGH_ACCURACY
//  // increase timing budget to 200 ms
//  sensor.setMeasurementTimingBudget(200000);
//#endif

void setup()
{
  Serial.begin(9600);
  // D9, D10
//  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);
  
  Wire.begin();
  Sensor1.setAddress(42);
  pinMode(10, INPUT);
  delay(10);
  Sensor2.setAddress(43);
  
  Sensor1.init();
  Sensor2.init();
  Sensor1.setTimeout(500);
  Sensor2.setTimeout(500);
//   Sensor1.startContinuous();
//  Sensor2.startContinuous();

}

void loop()
{
  //Sensor2.readRangeContinuousMillimeters();
  
  Serial.print(Sensor1.readRangeSingleMillimeters());
  if (Sensor1.timeoutOccurred()) { Serial.print(" Sensor 1 TIMEOUT"); }
  Serial.print(", ");
  Serial.print(Sensor1.getAddress());
  Serial.print(", ");
  Serial.print(Sensor2.readRangeSingleMillimeters());
  Serial.print(", ");
  if (Sensor2.timeoutOccurred()) { Serial.print(" Sensor 2 TIMEOUT"); }
  Serial.print(Sensor2.getAddress());
  
  Serial.println();
}

