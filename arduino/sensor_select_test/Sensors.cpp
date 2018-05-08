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

int Sensors::GetDistance(int sendsorNum){
  int count = Sensors::sensorCount;
  int distance = Sensors::sensors[sendsorNum].readRangeContinuousMillimeters();
  return distance;
}

void Sensors::SendDistance(HardwareSerial *refSerial, String id, int* distance){
  //sensor,[arduino id]:[distance]\n
  refSerial->print("distance,");
  refSerial->print(id);
  refSerial->print(":");
  for(int i = 0 ; i < Sensors::sensorCount-1; i++){
    refSerial->print(distance[i]);  
    refSerial->print(",");  
  }
  refSerial->println(distance[Sensors::sensorCount]);
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
