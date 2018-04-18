#include <Wire.h>
// Import VL53L0X Pololu Library v1.0.2
#include <VL53L0X.h>
#include "Sensors.h"
#include "Battery.h"

Sensors manager;
Battery batt;
//
//typedef enum {START_, WAIT_AT_, WAIT_RSSI_, WAIT_EXIT_, NORM_MODE_, TMP_} state_type;
//state_type state = START_;
//char chr;
//String buffer, rssi, data;
//bool reading = false;
//
//void setup() {
//   Serial.begin(115200);      //Initiate the Serial comm
//
//   state = START_;
//   manager = Sensors(2, 9, 10);
//   batt = Battery(5010, 3200, 900);
//}
//
//void loop() {
//   if (state == START_) {
//      start();
//   } else if (state == WAIT_AT_) {
//      wait_at();
//   } else if (state == WAIT_RSSI_) {
//      wait_rssi();
//   } else if (state == WAIT_EXIT_) {
//      wait_exit();
//   } else if (state == NORM_MODE_) {
//      norm_mode();
//   }
//}
//
//void start() {
//   delay(70);
//   Serial.print("+");
//   Serial.print("+");
//   Serial.print("+");
//   data = "";
//   state = WAIT_AT_;
//}
//
//void wait_at() {
//  delay(70);
//   while (Serial.available() > 0) {
//      chr = Serial.read();
//      if (chr == 'E') {
//         reading = true;
//         buffer = "";
//      }
//      if (reading) {
//         buffer += chr;
//      } else {
//         data += chr;
//      }
//      if (reading && buffer == "Enter AT Mode\r\n") {
//         Serial.println("AT+RSSI=?");
//         reading = false;
//         state = WAIT_RSSI_;
//      } 
//      else if (data == "led_on") {
////         digitalWrite(ledBLE, HIGH);
//         data = "";
//      } else if (data == "led_off") {
////         digitalWrite(ledBLE, LOW);
//         data = "";
//      }
//   }
//}
//
//void wait_rssi() {
//   while (Serial.available() > 0) {
//      chr = Serial.read();
//      if (chr == '-') {
//         reading = true;
//         buffer = "";
//      }
//      if (reading) {
//         buffer += chr;
//      }
//      if (buffer.length() == 4) {
//         rssi = buffer;
//         Serial.println("AT+EXIT");
//         reading = false;
//         state = WAIT_EXIT_;
//         if (rssi == "-000") {
////            digitalWrite(ledBLE, LOW);
//         }
//         else {
//            state = WAIT_EXIT_;
//         }
//      }
//   }
//}
//
//void wait_exit() {
//   while (Serial.available() > 0) {
//      chr = Serial.read();
//      if (chr == 'O') {
//         reading = true;
//         buffer = "";
//      }
//      if (reading) {
//         buffer += chr;
//      }
//      if (buffer == "OK\r\n") {
//         reading = false;
//         state = NORM_MODE_;
//      }
//   }
//}
//
//void norm_mode() {
//  if(rssi == "-000") {
//    
//  }
//  else 
//  {
//    Serial.println(rssi);
//    Serial.println(manager.GetDistance());
//    Serial.println(batt.ReadVcc());
//    Serial.flush();
//   
//  }
//   state = START_;
//}



void setup()
{
  Serial.begin(115200);
  manager = Sensors(2, 9, 10);
  batt = Battery(5010, 3200, 900);
}

void loop()
{ 
  if (Serial.available()>0) {
       Serial.println(manager.GetDistance());
       Serial.println(batt.ReadVcc());
    }
}

