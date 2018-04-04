#include <Wire.h>
// Import VL53L0X Pololu Library v1.0.2
#include <VL53L0X.h>

const int DISTANCE_THRESHOLD = 8190;
VL53L0X Sensor1;
VL53L0X Sensor2;
int distance1;
int distance2;
unsigned long time ;

void setup()
{
  Serial.begin(115200);
  // D9, D10
  //  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);

  Wire.begin();
  Sensor1.setAddress(42);
  pinMode(10, INPUT);
  delay(10);
  Sensor2.setAddress(43);

  Sensor1.init();
  Sensor2.init();
  // Sensing Speed
  Sensor1.setMeasurementTimingBudget(20000);
  Sensor2.setMeasurementTimingBudget(20000);
  Sensor1.setTimeout(500);
  Sensor2.setTimeout(500);
  Sensor1.startContinuous();
  Sensor2.startContinuous();
}

void loop()
{

  Serial.print(readVcc());
  Serial.println();

  distance1 = Sensor1.readRangeContinuousMillimeters();
  if (Sensor1.timeoutOccurred()) {
    //        Serial.print(" Sensor 1 TIMEOUT");
  }
  else if(distance1 < DISTANCE_THRESHOLD){
    Serial.print(distance1);
    Serial.print(", ");
    Serial.print(Sensor1.getAddress());
    Serial.println();

  }

  distance2 = Sensor2.readRangeContinuousMillimeters();
  if (Sensor2.timeoutOccurred()) {
    //        Serial.print(" Sensor 2 TIMEOUT");
  }
  else if(distance2 < DISTANCE_THRESHOLD){
    Serial.print(distance2);
    Serial.print(", ");
    Serial.print(Sensor2.getAddress());
    Serial.println();

  }
}

// Maximum 5028(mV). VIN Not Support(7~12V)
long readVcc() {
  long result;
  // Read 1.1V reference against AVcc
  ADMUX = _BV(REFS0) | _BV(MUX3) | _BV(MUX2) | _BV(MUX1);
  delay(2); // Wait for Vref to settle
  ADCSRA |= _BV(ADSC); // Convert
  while (bit_is_set(ADCSRA, ADSC));
  result = ADCL;
  result |= ADCH << 8;
  result = 1126400L / result; // Back-calculate AVcc in mV
  return result;
}
