#include "Arduino.h"
#include "Battery.h"
#include <SoftwareSerial.h>

// ----- public
Battery::Battery(){};
Battery::Battery(int maximum, int minimum, int cap){
  Battery::maximumVoltage = maximum;
  Battery::minimumVoltage = minimum;
  Battery::capacity = cap;
  Battery::RefreshPercentage();
}

int Battery::RefreshPercentage(SoftwareSerial *mySerial = NULL, String id = ""){
  // TODO 배터리 잔량 및 사용시간 추정 계산 해야함.
  int maximumV = Battery::maximumVoltage;
  int minimumV = Battery::minimumVoltage;
  int capacity = Battery::capacity;
  int currentV = ReadVcc();

  // TODO calculate percentage more exactly.

  int newPercentage = (currentV-minimumV)/(maximumV - minimumV);
  if(Battery::percentage != newPercentage){
    Battery::percentage = newPercentage;
    if(mySerial != NULL){
      Battery::SendBattery(mySerial, id);
    }
  }
  return Battery::percentage;
}

void Battery::SendBattery(SoftwareSerial *mySerial, String id){
  //batt,[arduino id]:[percentage]\n
  mySerial->print("batt,");
  mySerial->print(id);
  mySerial->print(":");
  mySerial->println(Battery::ReadVcc());
}

// ----- private
int Battery::ReadVcc(/*Seems need Wire*/){
  long result;
  // Read 1.1V reference against AVcc
  ADMUX = _BV(REFS0) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);
  delay(2); // Wait for Vref to settle
  ADCSRA |= _BV(ADSC); // Convert
  while (bit_is_set(ADCSRA, ADSC));
  result = ADCL;
  result |= ADCH << 8;
  result = 1126400L / result; // Back-calculate AVcc in mV
  return (int)result;
}

