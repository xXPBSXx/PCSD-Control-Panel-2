#include <WiFi.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Fonts/FreeSansBold18pt7b.h>

// WiFi credentials
const char* ssid = ""; //WiFi Name
const char* password = ""; //WiFi Password

// Set Time Zone
#define TZ_INFO "CET-1CEST,M3.5.0/2,M10.5.0/3"

// Screen settings
#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
#define STAR_COUNT 30
#define ROCKET_FIELD_WIDTH 10
#define ROCKET_FIELD_HEIGHT 20
#define ROCKET_VIBRATION 1

Adafruit_SSD1306 screen1(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);
Adafruit_SSD1306 screen2(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

int cpuTemp  = 0;
int cpuUsage = 0;
int gpuTemp  = 0;
int gpuUsage = 0;

struct Star {
  int x;
  int y;
  int speed;
};

struct Rocket {
  int x;
  int y;
  int baseX;
  int baseY;
  int speedY;
  int directionY;
  int vibrationDirectionX;
  int vibrationDirectionY;
};

Star stars[STAR_COUNT];
Rocket rocket1 = {15, 27, 15, 27, 1, 1, 1, 1};
Rocket rocket2 = {SCREEN_WIDTH - 15, 27, SCREEN_WIDTH - 15, 27, 1, -1, -1, -1};
Rocket rocket3 = {15, 27, 15, 27, 1, 1, 1, 1};
Rocket rocket4 = {SCREEN_WIDTH - 15, 27, SCREEN_WIDTH - 15, 27, 1, -1, -1, -1};
bool surpriseActive = false;
unsigned long surpriseStartTime = 0;

void initializeStars() {
  for (int i = 0; i < STAR_COUNT; i++) {
    stars[i].x = random(0, SCREEN_WIDTH);
    stars[i].y = random(0, SCREEN_HEIGHT - 10);
    stars[i].speed = random(1, 3);
  }
}

void updateStars() {
  for (int i = 0; i < STAR_COUNT; i++) {
    stars[i].y += stars[i].speed;
    if (stars[i].y >= SCREEN_HEIGHT - 10) {
      stars[i].y = 0;
      stars[i].x = random(0, SCREEN_WIDTH);
    }
  }
}

void drawStars(Adafruit_SSD1306 &display) {
  for (int i = 0; i < STAR_COUNT; i++) {
    display.drawPixel(stars[i].x, stars[i].y, SSD1306_WHITE);
  }
}

void updateRocket(Rocket &rocket) {
  rocket.y += rocket.speedY * rocket.directionY;
  if (rocket.y <= 20 || rocket.y >= SCREEN_HEIGHT - 20) {
    rocket.directionY *= -1;
    rocket.speedY = random(1, 2);
  }
  rocket.x += rocket.vibrationDirectionX * ROCKET_VIBRATION;
  rocket.y += rocket.vibrationDirectionY * ROCKET_VIBRATION;
  if (rocket.x > rocket.baseX + ROCKET_FIELD_WIDTH / 2 || rocket.x < rocket.baseX - ROCKET_FIELD_WIDTH / 2) {
    rocket.vibrationDirectionX *= -1;
  }
 
  if (rocket.y > rocket.baseY + ROCKET_FIELD_HEIGHT / 2 || rocket.y < rocket.baseY - ROCKET_FIELD_HEIGHT / 2) {
    rocket.vibrationDirectionY *= -1;
  }
}

void drawRocket(Adafruit_SSD1306 &display, Rocket &rocket) {
  display.fillRect(rocket.x - 2, rocket.y - 5, 5, 10, SSD1306_WHITE);
  display.drawTriangle(rocket.x - 3, rocket.y - 5, rocket.x + 3, rocket.y - 5, rocket.x, rocket.y - 10, SSD1306_WHITE);
  display.drawLine(rocket.x - 3, rocket.y + 3, rocket.x - 5, rocket.y + 6, SSD1306_WHITE);
  display.drawLine(rocket.x + 3, rocket.y + 3, rocket.x + 5, rocket.y + 6, SSD1306_WHITE);
  if (rocket.directionY < 0) {
    display.drawLine(rocket.x - 2, rocket.y + 10, rocket.x - 4, rocket.y + 13, SSD1306_WHITE);
    display.drawLine(rocket.x + 2, rocket.y + 10, rocket.x + 4, rocket.y + 13, SSD1306_WHITE);
    display.drawLine(rocket.x, rocket.y + 10, rocket.x, rocket.y + 15, SSD1306_WHITE);
  }
}

void triggerSurprise() {
  surpriseActive = true;
  surpriseStartTime = millis();
}

void drawFirework(Adafruit_SSD1306 &display) {
  if (millis() - surpriseStartTime < 5000) {
    display.clearDisplay();
    for (int i = 0; i < 10; i++) {
      int x = random(20, SCREEN_WIDTH - 20);
      int y = random(20, SCREEN_HEIGHT - 20);
      int r = random(5, 15);
      display.drawCircle(x, y, r, SSD1306_WHITE);
    }
    display.display();
  } else {
    surpriseActive = false;
  }
}


const int serialBufLen = 24;
char serialBuf[serialBufLen];
byte serialPos = 0;

void checkSerial() {
  while (Serial.available()) {
    char c = Serial.read();
    if (serialPos < 16) {
      serialBuf[serialPos++] = c;
    }
    if (serialPos == 16) { 
      serialBuf[serialPos] = '\0'; 
      parseSensorData(serialBuf);
      serialPos = 0;
    }
  }
}

void parseSensorData(const char* buf) {
  String staticData = String(buf);
  cpuTemp  = staticData.substring(1,4).toInt();
  cpuUsage = staticData.substring(5,8).toInt();
  gpuTemp  = staticData.substring(9,12).toInt();
  gpuUsage = staticData.substring(13,16).toInt();
}


void setup() {
  Serial.begin(115200); //Set Baud rate
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
  }
  configTzTime(TZ_INFO, "pool.ntp.org");
  Wire.begin();
  screen1.begin(SSD1306_SWITCHCAPVCC, 0x3C);
  screen1.clearDisplay();
  screen1.display();
  screen2.begin(SSD1306_SWITCHCAPVCC, 0x3D);
  screen2.clearDisplay();
  screen2.display();
  initializeStars();
}


int textWidth(Adafruit_SSD1306 &display, String s) {
  int16_t x, y;
  uint16_t w, h;
  display.getTextBounds(s, 0, 0, &x, &y, &w, &h);
  return w;
}


unsigned long lastAnimationUpdate = 0;
const unsigned long animationInterval = 1;

void loop() {
  checkSerial();

  if (millis() - lastAnimationUpdate >= animationInterval) {
    lastAnimationUpdate = millis();

    updateStars();
    updateRocket(rocket1);
    updateRocket(rocket2);
    updateRocket(rocket3);
    updateRocket(rocket4);

    struct tm timeinfo;
    getLocalTime(&timeinfo);

    int hours = timeinfo.tm_hour;
    int minutes = timeinfo.tm_min;

    static int lastCheckedMinute = -1;
    if (minutes % 30 == 0 && minutes != lastCheckedMinute) {
      triggerSurprise();
      lastCheckedMinute = minutes;
    }

    
    String cpuInfo = "CPU: " + String(cpuTemp) + (char)247 + "C  " + String(cpuUsage) + "%";
    String gpuInfo = "GPU: " + String(gpuTemp) + (char)247 + "C  " + String(gpuUsage) + "%";

    
    screen1.setFont(&FreeSansBold18pt7b);
    screen2.setFont(&FreeSansBold18pt7b);

    
    int visibleHeight = SCREEN_HEIGHT - 12;

    if (surpriseActive) {
      screen1.clearDisplay();
      screen2.clearDisplay();
      drawFirework(screen1);
      drawFirework(screen2);
    } else {
      
      screen1.clearDisplay();
      
      for (int i = 0; i < STAR_COUNT; i++) {
        if (stars[i].y < visibleHeight) screen1.drawPixel(stars[i].x, stars[i].y, SSD1306_WHITE);
      }
      drawRocket(screen1, rocket1);
      drawRocket(screen1, rocket2);

      
      String hoursStr = (hours < 10) ? "0" + String(hours) : String(hours);
      int16_t x1, y1;
      uint16_t w, h;
      screen1.getTextBounds(hoursStr, 0, 0, &x1, &y1, &w, &h);
      screen1.setCursor((SCREEN_WIDTH - w) / 2, (visibleHeight - h) / 2 - y1 + 8);
      screen1.setTextColor(SSD1306_WHITE);
      screen1.print(hoursStr);

     
      screen1.setFont(); 
      int cpuW = textWidth(screen1, cpuInfo);
      int cpuX = (SCREEN_WIDTH - cpuW) / 2;
      screen1.setCursor(cpuX, SCREEN_HEIGHT - 8);
      screen1.print(cpuInfo);

      screen1.display();

     
      screen2.clearDisplay();
      for (int i = 0; i < STAR_COUNT; i++) {
        if (stars[i].y < visibleHeight) screen2.drawPixel(stars[i].x, stars[i].y, SSD1306_WHITE);
      }
      drawRocket(screen2, rocket3);
      drawRocket(screen2, rocket4);

      String minutesStr = (minutes < 10) ? "0" + String(minutes) : String(minutes);
      screen2.getTextBounds(minutesStr, 0, 0, &x1, &y1, &w, &h);
      screen2.setCursor((SCREEN_WIDTH - w) / 2, (visibleHeight - h) / 2 - y1 + 8);
      screen2.setTextColor(SSD1306_WHITE);
      screen2.print(minutesStr);

      screen2.setFont();
      int gpuW = textWidth(screen2, gpuInfo);
      int gpuX = (SCREEN_WIDTH - gpuW) / 2;
      screen2.setCursor(gpuX, SCREEN_HEIGHT - 8);
      screen2.print(gpuInfo);

      screen2.display();
    }
  }
}