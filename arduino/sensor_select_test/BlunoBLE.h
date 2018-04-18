#ifndef BlunoBLE_h
#define BlunoBLE_H

#include "Arduino.h"

class BlunoBLE{
  public:
    void Send(String text);
    // void Connected();
    // void Disconnected();
  private:
    HardwareSerial *mySerial;
};

#endif
