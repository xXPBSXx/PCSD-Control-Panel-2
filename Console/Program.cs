using System;
using System.Threading;
using LibreHardwareMonitor.Hardware;

internal class Program
{
    static void Main(string[] args)
    {
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true
        };

        computer.Open();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Was möchtest du ansehen?");
            Console.WriteLine("1) CPU Temperatur (Tctl/Tdie)");
            Console.WriteLine("2) RAM Nutzung");
            Console.WriteLine("3) GPU Temperatur");
            Console.WriteLine("4) Alle Sensoren anzeigen");
            Console.WriteLine("5) Beenden");
            Console.Write("Deine Wahl: ");
            string input = Console.ReadLine() ?? ""; // Warnung vermeiden

            if (input == "1")
            {
                int temp = GetCpuTctlTdie(computer);
                if (temp == int.MinValue)
                    Console.WriteLine("CPU-Temperatur nicht gefunden!");
                else
                    Console.WriteLine($"CPU Temperatur (Tctl/Tdie): {temp} °C");
            }
            else if (input == "2")
            {
                PrintRamUsage(computer);
            }
            else if (input == "3")
            {
                PrintGpuTemp(computer);
            }
            else if (input == "4")
            {
                PrintAllSensors(computer);
            }
            else if (input == "5")
            {
                break;
            }
            else
            {
                Console.WriteLine("Ungültige Eingabe!");
            }

            Console.WriteLine("\nWeiter mit <Enter>...");
            Console.ReadLine();
        }

        computer.Close();
    }

    // CPU Temperatur (Tctl/Tdie)
    static int GetCpuTctlTdie(Computer computer)
    {
        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature &&
                        sensor.Name == "Core (Tctl/Tdie)")
                    {
                        var value = sensor.Value;
                        if (value.HasValue)
                            return (int)Math.Round(value.Value);
                    }
                }
            }
        }
        return int.MinValue;
    }

    // RAM-Nutzung anzeigen
    static void PrintRamUsage(Computer computer)
    {
        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Memory)
            {
                hardware.Update();
                float? used = null;
                float? available = null;
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Name == "Memory Used" && sensor.Value.HasValue)
                        used = sensor.Value;
                    if (sensor.Name == "Memory Available" && sensor.Value.HasValue)
                        available = sensor.Value;
                }

                if (used != null && available != null)
                {
                    Console.WriteLine($"RAM benutzt: {used:F0} MB");
                    Console.WriteLine($"RAM verfügbar: {available:F0} MB");
                }
                else
                {
                    Console.WriteLine("RAM-Daten nicht gefunden!");
                }
            }
        }
    }

    // GPU Temperatur anzeigen (nur NVIDIA)
    static void PrintGpuTemp(Computer computer)
    {
        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.GpuNvidia)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature &&
                        sensor.Name == "GPU Core" &&
                        sensor.Value.HasValue)
                    {
                        Console.WriteLine($"GPU Temperatur: {sensor.Value.Value:F1} °C");
                        return;
                    }
                }
            }
        }
        Console.WriteLine("GPU Temperatur nicht gefunden!");
    }

    // Alle Sensoren und Werte auflisten
    static void PrintAllSensors(Computer computer)
    {
        Console.WriteLine("Alle erkannten Sensoren:");
        foreach (var hardware in computer.Hardware)
        {
            hardware.Update();
            Console.WriteLine($"{hardware.HardwareType}: {hardware.Name}");
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.Value.HasValue)
                    Console.WriteLine($"   {sensor.Name} [{sensor.SensorType}]: {sensor.Value.Value} {(sensor.SensorType == SensorType.Temperature ? "°C" : "")}");
            }
        }
    }
}