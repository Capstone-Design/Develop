#include "Arduino.h"
#include "Battery.h"
#include <math.h>
#include <SoftwareSerial.h>

// ----- public
Battery::Battery(){
  Battery::maximumVoltage = 4100;
  Battery::minimumVoltage = 3600;
  Battery::capacity = 900;
  Battery::RefreshPercentage();
}

int Battery::RefreshPercentage(SoftwareSerial *mySerial = NULL){
  // TODO 배터리 잔량 및 사용시간 추정 계산 해야함.
  int maximumV = Battery::maximumVoltage;
  int minimumV = Battery::minimumVoltage;
  int capacity = Battery::capacity;
  int currentV = ReadVcc();
  int newPercentage;
  double voltage = currentV/1000.0;
  
  if(currentV > maximumV){
    Battery::percentage = 100;
  }
  else {
    // Voltage = 2^(-Percentage/4)*4.1
    newPercentage = 4*(log(4.1) - log(voltage))/log(2);
  }

  if(Battery::percentage != newPercentage){
    Battery::percentage = newPercentage;
    if(mySerial != NULL){
      for(int i = 0 ; i < 10; i ++){
        Battery::SendBattery(mySerial);  
      }
    }
  }
  return Battery::percentage;
}

void Battery::SendBattery(SoftwareSerial *mySerial){
  //batt,[arduino id]:[percentage]\n
  mySerial->print("batt,");
  mySerial->print(":");
  mySerial->println(Battery::percentage);
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

