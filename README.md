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
* PC info only

**Important:** For both clock versions, make sure to adjust the `// WiFi credentials` section in the Arduino code. If the credentials are not set correctly, the displays will remain black.

---

## Note

This is not an official release of PCSD Control Panel 2. It is only a functional modification for better sensor support.

Original repository: [https://github.com/Shift2Ace/PCSD-Control-Panel-2](https://github.com/Shift2Ace/PCSD-Control-Panel-2)
