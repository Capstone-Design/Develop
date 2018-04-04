#include "Arduino.h"


// ----- public
Battery::Battery(int maximum, int minimum, int cap, int arduinoId = -1){
  Battery::maximumVoltage = maximum;
  Battery::minimumVoltage = minimum;
  Battery::capacity = cap;
  Battery::arduinoId = arduinoId;
}

int Battery::RefreshPercentage(HardwareSerial &mySerial = NULL){
  int maximumV = Battery::maximumVoltage;
  int minimumV = Battery::minimumVoltage;
  int capacity = Battery::capacity;
  int currentV = ReadVcc();

  // TODO calculate percentage more exactly.

  int newPercentage = (currentV-minimumV)/(maximumV - minimumV);
  if(Battery::percentage != newPercentage){
    Battery::percentage = newPercentage;
    if(mySerial != NULL){
      Battery::SendBattery(mySerial, Battery::arduinoId);
    }
  }
  return Battery::percentage;
}

void Battery::SendBattery(HardwareSerial &mySerial, String id){
  //batt,[arduino id]:[percentage]\n
  refSerial.print("distance,");
  refSerial.print(id);
  refSerial.print(":");
  refSerial.println(distance);
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

#endif
