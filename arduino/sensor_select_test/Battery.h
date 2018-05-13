#ifndef Battery_h
#define Battery_h

#include "Arduino.h"
#include <SoftwareSerial.h>

class Battery{
  public:
    Battery();
    // Set values
    Battery(int maximum, int minimum, int cap);
    // Send data with format
    void SendBattery(SoftwareSerial *mySerial, String id);
    // Update percentage
    // if mySerial != NULL and percentage changed, call SendBattery.
    int RefreshPercentage(SoftwareSerial *mySerial = NULL, String id ="");
    
    // For bluno, maximum of ReadVcc is 5028(mV).
    // This function not support VIN(7~12V)
    // return : Volage
    int ReadVcc();
  private:
    int voltage;          // (V)
    int capacity;         // (mAh)
    int maximumVoltage;   // (V)
    int minimumVoltage;   // (V)
    int percentage;       // 0~100(%)


    
};

#endif
