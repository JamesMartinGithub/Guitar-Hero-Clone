/*
  Copyright (c) 2022 Marc Khouri (https://marc.khouri.ca/posts/2022/GH5-neck-comms.html)

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/
#include <Wire.h>

// Toggles
#define SERIAL_EN true  // Enable serial output

// Constants / structs
#define I2C_ADDRESS 0x0D
const char * hex = "0123456789ABCDEF";
typedef struct {
  bool green;
  bool red;
  bool yellow;
  bool blue;
  bool orange;
  bool greenSlide;
  bool redSlide;
  bool yellowSlide;
  bool blueSlide;
  bool orangeSlide;
} Buttons;
char output[15] = {'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '1', '0', '0', '0', 'U'};

int barUp = A0;
int barDown = A1;
int upLast = 999;
int downLast = 999;
int led = 7;
int whammy = A2;
int select = A3;

void setup() {
  Wire.begin();  // join i2c bus (address optional for master)
  pinMode(led, OUTPUT);
  if (SERIAL_EN) {
    Serial.begin(115200);
    Serial.println("Setup complete");
  }
}

void diagnoseTransmissionError(byte code) {
  // Docs for wire.endtransmission: https://docs.particle.io/cards/firmware/wire-i2c/endtransmission/
  switch (code) {
    case 0:
      Serial.println("success");
      break;
    case 1:
      Serial.println("busy timeout upon entering endTransmission()");
      break;
    case 2:
      Serial.println("START bit generation timeout");
      break;
    case 3:
      Serial.println("end of address transmission timeout");
      break;
    case 4:
      Serial.println("data byte transfer timeout");
      break;
    case 5:
      Serial.println("data byte transfer succeeded, busy timeout immediately after");
      break;
    case 6:
      Serial.println("timeout waiting for peripheral to clear stop bit");
      break;
    default:
      Serial.print("Unknown return from EndTransmission: ");
      Serial.println(code);
  }
}

unsigned int readFromSerial(uint8_t* arr, unsigned int expectedByteCount) {
  unsigned int readCount = 0;
  Wire.requestFrom(I2C_ADDRESS, expectedByteCount);    // request N bytes from peripheral device
  while (Wire.available() && (readCount < expectedByteCount)) { // peripheral may send less than requested
    arr[readCount] = Wire.read(); // receive a byte as character
    readCount++;
  }
  return readCount;
}

void printByteArray(uint8_t* arr, unsigned int len) {
  Serial.print("Read ");
  Serial.print(len);
  Serial.print(" bytes:");
  for (unsigned int i = 0; i < len; i++) {
    Serial.print(" 0x");
    Serial.print(hex[(arr[i] >> 4) & 0xF]);
    Serial.print(hex[arr[i] & 0xF]);
  }
  Serial.println("");
}

Buttons twoBytesToButton(uint8_t* arr) {
  Buttons buttons = {.green = false, .red = false, .yellow = false, .blue = false, .orange = false, .greenSlide = false, .redSlide = false, .yellowSlide = false, .blueSlide = false, .orangeSlide = false};
  char topButtons = arr[0];
  if (topButtons & 0x10) {
    buttons.green = true;
  }
  if (topButtons & 0x20) {
    buttons.red = true;
  }
  if (topButtons & 0x80) {
    buttons.yellow = true;
  }
  if (topButtons & 0x40) {
    buttons.blue = true;
  }
  if (topButtons & 0x01) {
    buttons.orange = true;
  }

  // The remaining array on the real guitar 5 bytes long. It varies in a stable
  // way when pressing on the touch pad. However, we can get away with just
  // looking at the first byte of the five, since it has a unique value for
  // each combination of touchpad presses.
  // There's gotta be a pattern here, but I'm too tired to spot it...
  switch (arr[1]) {
    case 0x00:
      // no buttons
      break;
    case 0x95:
      buttons.greenSlide = true; break;
    case 0xCD:
      buttons.redSlide = true; break;
    case 0x1A:
      buttons.yellowSlide = true; break;
    case 0x49:
      buttons.blueSlide = true; break;
    case 0x7F:
      buttons.orangeSlide = true; break;
    case 0xB0:
      buttons.greenSlide = true; buttons.redSlide = true; break;
    case 0x19:
      buttons.greenSlide = true; buttons.yellowSlide = true; break;
    case 0x47:
      buttons.greenSlide = true; buttons.blueSlide = true; break;
    case 0x7B:
      buttons.greenSlide = true; buttons.orangeSlide = true; break;
    case 0xE6:
      buttons.redSlide = true; buttons.yellowSlide = true; break;
    case 0x48:
      buttons.redSlide = true; buttons.blueSlide = true; break;
    case 0x7D:
      buttons.redSlide = true; buttons.orangeSlide = true; break;
    case 0x2F:
      buttons.yellowSlide = true; buttons.blueSlide = true; break;
    case 0x7E:
      buttons.yellowSlide = true; buttons.orangeSlide = true; break;
    case 0x66:
      buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x65:
      buttons.yellowSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x64:
      buttons.redSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x7C:
      buttons.redSlide = true; buttons.yellowSlide = true; buttons.orangeSlide = true; break;
    case 0x2E:
      buttons.redSlide = true; buttons.yellowSlide = true; buttons.blueSlide = true; break;
    case 0x62:
      buttons.greenSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x7A:
      buttons.greenSlide = true; buttons.yellowSlide = true; buttons.orangeSlide = true; break;
    case 0x2D:
      buttons.greenSlide = true; buttons.yellowSlide = true; buttons.blueSlide = true; break;
    case 0x79:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.orangeSlide = true; break;
    case 0x46:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.blueSlide = true; break;
    case 0xE5:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.yellowSlide = true; break;
    case 0x63:
      buttons.redSlide = true; buttons.yellowSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x61:
      buttons.greenSlide = true; buttons.yellowSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x60:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    case 0x78:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.yellowSlide = true; buttons.orangeSlide = true; break;
    case 0x2C:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.yellowSlide = true; buttons.blueSlide = true; break;
    case 0x5F:
      buttons.greenSlide = true; buttons.redSlide = true; buttons.yellowSlide = true; buttons.blueSlide = true; buttons.orangeSlide = true; break;
    default:
      if (SERIAL_EN) {
        Serial.print("Unrecognized pattern! ");
        printByteArray(&arr[1], 1);
      }
  }

  return buttons;
}

void printButtons(Buttons buttons) {
  strncpy(output, "00000000001000U", 15);
  if (buttons.green) {output[0] = '1';}
  if (buttons.red) {output[1] = '1';}
  if (buttons.yellow) {output[2] = '1';}
  if (buttons.blue) {output[3] = '1';}
  if (buttons.orange) {output[4] = '1';}
  if (buttons.greenSlide) {output[5] = '1';}
  if (buttons.redSlide) {output[6] = '1';}
  if (buttons.yellowSlide) {output[7] = '1';}
  if (buttons.blueSlide) {output[8] = '1';}
  if (buttons.orangeSlide) {output[9] = '1';}
  int upRead = analogRead(barUp);
  int downRead = analogRead(barDown);
  if (upRead == 0 && upLast == 0) {output[10] = '2';}
  if (downRead == 0 && downLast == 0) {output[10] = '0';}
  int whammyRead = analogRead(whammy);
  output[11] = hex[(whammyRead / 256) % 256];
  output[12] = hex[(whammyRead / 16) % 16];
  output[13] = hex[whammyRead % 16];
  if (analogRead(select) < 15) {output[14] = 'S';}
  Serial.println(output);
  upLast = upRead;
  downLast = downRead;
}

bool isInitialized = false;
unsigned int loopCounter = 0; // Only used to control how often debug output is printed
void loop() {
  if (!isInitialized) {
    delay(1000);
    if (SERIAL_EN) {
      Serial.println("Not yet initialized");
    }
    Wire.beginTransmission(I2C_ADDRESS);  // Transmit to device
    Wire.write(0x00);             // Sends value byte
    byte error = Wire.endTransmission();      // Stop transmitting
    if (error != 0) {
      if (SERIAL_EN) {
        diagnoseTransmissionError(error);
      }
      return;
    }

    unsigned int expectedInitBytes = 7;
    uint8_t values[expectedInitBytes];
    unsigned int readCount = readFromSerial(values, expectedInitBytes);
    if (readCount == expectedInitBytes) {
      if (SERIAL_EN) {
        Serial.println("Initialized");
      }
      isInitialized = true;
      digitalWrite(led, HIGH);
    } else {
      if (SERIAL_EN) {
        Serial.println("Wrong byte count read");
      }
    }
    return;
  }
  
  delay(0.5); // The guitar leaves a 9ms gap between reads, but it seems like we can go lower

  Wire.beginTransmission(0x0D);  // Transmit to device
  Wire.write(0x12);             // Sends value byte to set the memory address we want to read from
  byte error = Wire.endTransmission();      // Stop transmitting
  if (error != 0) {
    if (SERIAL_EN) {
      diagnoseTransmissionError(error);
    }
    return;
  }

  // The guitar will send up to 6 bytes in this response, but we only need the first
  // one for the buttons and the second byte to decode the touchpad. The remaining
  // bytes seems useless.
  unsigned int expectedByteCount = 2;
  uint8_t values[expectedByteCount];
  unsigned int readCount = readFromSerial(values, expectedByteCount);
  if (readCount != expectedByteCount) {
    if (SERIAL_EN) {
      Serial.println("Wrong byte count read");
    }
    return;
  }
  Buttons buttons = twoBytesToButton(values);
  if (SERIAL_EN) {
    loopCounter++;
    // Only print out the button state periodically, so polling stays fast
    if (loopCounter % 25 == 0) {
      printButtons(buttons);
    }
  }
}
