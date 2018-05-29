#ifndef Battery_h
#define Battery_h

#include "Arduino.h"
#include <SoftwareSerial.h>

class Battery{
  public:
    Battery();
    // Send data with format
    void SendBattery(SoftwareSerial *mySerial);
    // Update percentage
    // if mySerial != NULL and percentage changed, call SendBattery.
    int RefreshPercentage(SoftwareSerial *mySerial = NULL);
    
    // For bluno, maximum of ReadVcc is 5028(mV).
    // This function not support VIN(7~12V)
    // return : Volage
    int ReadVcc();
  private:
    int capacity;         // (mAh)
    int maximumVoltage;   // (V)
    int minimumVoltage;   // (V)
    int percentage;       // 0~100(%)


    
};

#endif
