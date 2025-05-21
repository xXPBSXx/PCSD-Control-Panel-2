#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

int cpuTemp  = 0;
int cpuUsage = 0;
int gpuTemp  = 0;
int gpuUsage = 0;
int ramUsed  = 0;
int ramTotal = 0;

String staticData = "";

int prevCpuTemp = -1;
int prevCpuUsage = -1;
int prevGpuTemp = -1;
int prevGpuUsage = -1;
int prevRamUsed = -1;
int prevRamTotal = -1;

void setup() {
  Serial.begin(115200); //Set Baud rate
  Wire.begin();
  display.begin(SSD1306_SWITCHCAPVCC, 0x3C);
  display.clearDisplay();
  display.setTextSize(1);
  display.setTextColor(SSD1306_WHITE);
  display.setCursor(0,0);
  display.print("PC Info Monitor");
  display.display();
  delay(1000);
  display.clearDisplay();
  printStaticLabels();
}

void loop() {
  if (Serial.available() > 0) {
    staticData = Serial.readString();
    cpuTemp  = staticData.substring(0,3).toInt();
    cpuUsage = staticData.substring(3,6).toInt();
    gpuTemp  = staticData.substring(6,9).toInt();
    gpuUsage = staticData.substring(9,12).toInt();
    if(staticData.length() >= 22) {
      ramUsed = staticData.substring(12,17).toInt();
      ramTotal = staticData.substring(17,22).toInt();
    }
    if (cpuTemp >= 0 && cpuUsage >= 0 && gpuTemp >= 0 && gpuUsage >= 0) {
      displayStatic();
    } else {
      displayError(1);
    }
  }
}

void printStaticLabels() {
  display.clearDisplay();
  display.setCursor(0,0);
  display.print("CPU:");
  display.setCursor(0,20);
  display.print("GPU:");
  display.display();
}

void displayError(int code) {
  display.clearDisplay();
  display.setCursor(0,0);
  display.print("Error TwT");
  display.setCursor(0,20);
  switch (code) {
    case 1:
      display.print("No Data");
      break;
  }
  display.display();
}

void displayStatic() {
  display.clearDisplay();
  // CPU
  display.setCursor(0,0);
  display.print("CPU:");
  display.setCursor(40,0);
  display.print(cpuTemp);
  display.print((char)247);
  display.print("C ");
  display.print(cpuUsage);
  display.print("%");
  // GPU
  display.setCursor(0,20);
  display.print("GPU:");
  display.setCursor(40,20);
  display.print(gpuTemp);
  display.print((char)247);
  display.print("C ");
  display.print(gpuUsage);
  display.print("%");
  // RAM
  if(ramUsed > 0 && ramTotal > 0) {
    float ramUsedGB = ramUsed / 1024.0;
    float ramTotalGB = ramTotal / 1024.0;
    display.setCursor(0, 40);
    display.print("RAM: ");
    display.print(ramUsedGB, 2);
    display.print("GB/");
    display.print(ramTotalGB, 2);
    display.print("GB");
  }
  display.display();
}