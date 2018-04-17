#include "Arduino.h"
#include "Sensors.h"
#include <VL53L0X.h>
#include <Wire.h>


// ----- public
Sensors::Sensors(){}
Sensors::Sensors(int count, ...){
  Sensors::sensorCount = count;
  Sensors::sensors = new VL53L0X[count];
  Sensors::sensorXshutPins = new int[count];
  
  va_list l_Arg;
  va_start(l_Arg,count);
  for (int i = 0; i < count; i++)
  {
    Sensors::sensorXshutPins[i] = va_arg(l_Arg, int);
  }
  va_end(l_Arg);
  Sensors::DisableAllSensor();
  Wire.begin();
  for (int i = 0; i < count; i++)
  {
    Sensors::SetSensor(i);
  }
}

int Sensors::GetDistance(){
  int count = Sensors::sensorCount;
  int distance;
  int sum = 0;
  int fineValue = 0;
  for(int i=0; i< count; i++, distance = 0){
    distance = Sensors::sensors[i].readRangeContinuousMillimeters();
    if (!Sensors::sensors[i].timeoutOccurred() && distance < DISTANCE_THRESHOLD) {
      sum += distance;
      fineValue++;
    }
  }
  // check fineValue == 0;
  return sum/fineValue;
}

void Sensors::SendDistance(HardwareSerial *refSerial, String id, String distance){
  //sensor,[arduino id]:[distance]\n
  refSerial->print("distance,");
  refSerial->print(id);
  refSerial->print(":");
  refSerial->println(distance);
}

// ----- private

void Sensors::DisableAllSensor(){
  for(int i=0; i< Sensors::sensorCount; i++){
    pinMode(sensorXshutPins[i], OUTPUT);
    digitalWrite(sensorXshutPins[i], LOW);
  }
}

void Sensors::SetSensor(int num){
  digitalWrite(Sensors::sensorXshutPins[num], HIGH); //begin writing to XSHUT of first laser
  delay(50); //delay for xshut apply
  Sensors::sensors[num].init();
  Sensors::sensors[num].setMeasurementTimingBudget(20000);
  Sensors::sensors[num].setAddress(0x21+num);
  Sensors::sensors[num].startContinuous();
}
