# Modified PCSD Control Panel 2 (LibreHardwareMonitor)

This folder contains a modified version of **PCSD Control Panel 2**, which uses **LibreHardwareMonitorLib** instead of **OpenHardwareMonitorLibCore** to improve hardware compatibility.

This is useful if the original version does not show temperatures or usage correctly, especially on some AMD CPUs or newer GPUs.

---

## What was changed

* Replaced references to `OpenHardwareMonitorLibCore` with `LibreHardwareMonitorLib`
* Adjusted `Form1.cs` and `Program.cs` to use the new library
* Updated NuGet packages in the `.csproj` files

---

## Included files

* `PCSD Control Panel 2.0.sln` – solution file
* `console/Program.cs` – modified to use LibreHardwareMonitor
* `console/console.csproj` – updated project file
* `PCSD Control Panel/Form1.cs` – updated UI and logic
* `PCSD Control Panel/PCSD Control Panel.csproj` – updated project file
* `ESP32-SSD1306/` – folder with Arduino `.ino` examples

---

## Requirements

* Windows PC
* Visual Studio 2022 Community Edition
* .NET Framework (as required by the original PCSD version)

---

## How to build

1. Open `PCSD Control Panel 2.0.sln` in Visual Studio 2022
2. Restore NuGet packages if needed
3. Build the solution
4. Run the application

It will now use LibreHardwareMonitor to read PC stats.

---

## Usage with Arduino (optional)

If you want to display the hardware info on an ESP32 with SSD1306 displays, use the Arduino code found in the `ESP32-SSD1306` folder of this repository.

Choose the version that fits your setup:

* Clock and animation only
* Clock and animation with PC info
* Clock and animation with PC info (Old)
* PC info only

**Important:** For all clock versions, make sure to adjust the `// WiFi credentials` section in the Arduino code. If the credentials are not set correctly, the displays will remain black.

**Baud Rate Warning:**
Make sure to set the correct baud rate in your Arduino code depending on the device and configuration you're using (e.g. 115200 for ESP32). This must also match the value set in the PCSD Control Panel. If they don't match, data transfer won't work properly. (e.g. 115200). Otherwise, data won't be read properly.

**Timezone Warning:**
Set the correct timezone in the Arduino code by adjusting the `#define TZ_INFO` value. Example for Central European Time:

```cpp
#define TZ_INFO "CET-1CEST,M3.5.0/2,M10.5.0/3"
```
More time zones can be found here:
[https://github.com/nayarsystems/posix\_tz\_db/blob/master/zones.csv](https://github.com/nayarsystems/posix_tz_db/blob/master/zones.csv)

---

## Arduino Code Descriptions

Inside the `ESP32-SSD1306` folder you'll find multiple `.ino` files:

### Clock\_Rocket\_Only.ino

* Displays current hour and minute on two OLEDs
* Includes simple rocket animation
* No PC connection required

### Clock\_Rocket\_PC\_Info.ino

* Same as above, but also displays CPU, GPU, and RAM usage
* Works with **modified PCSD (LibreHardwareMonitorLib)** only

### Clock\_Rocket\_PC\_Info\_Old.ino

* Displays clock, rocket animation, and CPU & GPU info (no RAM)
* Works with **original PCSD (OpenHardwareMonitorLibCore)** only
* Use this if you're using the unmodified version from the official repo

### PC\_Info\_Only.ino

* Only shows CPU, GPU and RAM info
* No clock or animations

---

## Note

This is not an official release of PCSD Control Panel 2. It is only a functional modification for better sensor support.

Original repository: [https://github.com/Shift2Ace/PCSD-Control-Panel-2](https://github.com/Shift2Ace/PCSD-Control-Panel-2)
