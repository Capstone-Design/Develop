#include "Arduino.h"
#include "Sensors.h"
#include <VL53L0X.h>
#include <Wire.h>


// ----- public

void Sensors::Init(int count, ...){
  Sensor::sensorCount = count;
  Sensor::sensors = new VL53L0X[sensorCount];
  Sensor::sensorXshutPins = new int[sensorCount];

  int num;
  va_list l_Arg;
  va_start(l_Arg,count);
  for (int i = 0; i < sensorCount; i++)
  {
    Sensor::sensorXshutPins[i] = va_arg(l_Arg, int);
  }
  va_end(l_Arg);
  Sensor::DisableAllSensor();
  Wire.begin();
  for (int i = 0; i < sensorCount; i++)
  {
    Sensor::SetSensor(i);
  }
}

int Sensor::GetDistance(){
  int count = Sensor::sensorCount;
  int sum = 0, fineValue = 0;
  for(int i=0; i< count; i++){
    distance[count] = Sensor::sensors[count].readRangeContinuousMillimeters();
    if (!Sensor::sensors[count].timeoutOccurred()) {
      sum += distance[count];
      fineValue++;
    }
  }
  return sum/fineValue;
}

void Sensors::SendDistance(HardwareSerial &refSerial, String id, String distance){
  //sensor,[arduino id]:[distance]\n
  refSerial.print("distance,");
  refSerial.print(id);
  refSerial.print(":");
  refSerial.println(distance);
}

// ----- private

void Sensors::DisableAllSensor(){
  for(int i=0; i< Sensor::sensorCount; i++){
    pinMode(sensorXshutPins[i], OUTPUT);
    digitalWrite(sensorXshutPins[i], LOW);
  }
}

void Sensor::SetSensor(int num){
  digitalWrite(sensorXshutPins[num], HIGH); //begin writing to XSHUT of first laser
  delay(50); //delay for xshut apply
  Sensor::sensors[num].init();
  Sensor::sensors[num].setMeasurementTimingBudget(20000);
  Sensor::sensors[num].setAddress(0x21+num);
  Sensor::sensors[num].startContinuous();
}
