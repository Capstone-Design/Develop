#ifndef Battery_h
#define Battery_h

#include "Arduino.h"

class Battery{
  public:
    // Set values
    Battery(int maximum, int minimum, int cap, int autoSend);
    // Send data with format
    void SendBattery(HardwareSerial &mySerial, String id);
    // Update percentage
    // if mySerial != NULL and percentage changed, call SendBattery.
    int RefreshPercentage(HardwareSerial &mySerial = NULL);
  private:
    int voltage;          // (V)
    int capacity;         // (mAh)
    int maximumVoltage;   // (V)
    int minimumVoltage;   // (V)
    int percentage;       // 0~100(%)

    // For bluno, maximum of ReadVcc is 5028(mV).
    // This function not support VIN(7~12V)
    // return : Volage
    int ReadVcc();
}

#endif
