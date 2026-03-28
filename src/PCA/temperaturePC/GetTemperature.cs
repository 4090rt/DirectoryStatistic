using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.temperaturePC
{
    public class GetTemperature
    {
        private static Computer _computer;

        public static void Initialize()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsPsuEnabled = true,
                IsControllerEnabled = true,
                IsBatteryEnabled = true,
            };
            _computer.Open();
        }
        public static Dictionary<string, List<Model>> Getemp()
        {
            try
            {
                var temperatures = new Dictionary<string, List<Model>>();

                if (_computer == null) Initialize();

                foreach (var hardware in _computer.Hardware)
                {
                    hardware.Update();

                    foreach (var sensor in hardware.Sensors)
                    {

                        // 1. Официальный температурный датчик
                        bool isTempByType = (int)sensor.SensorType == 4;

                        // 2. Имя содержит указание на температуру
                        bool isTempByName = sensor.Name.ToLower().Contains("temp") ||
                                           sensor.Name.Contains("°C") ||
                                           sensor.Name.Contains("Core") && !sensor.Name.Contains("Clock") ||
                                           sensor.Name.Contains("Package") ||
                                           sensor.Name.Contains("Hot Spot");

                        // 3. Значение в диапазоне температур (10-120°C)
                        bool isTempByValue = sensor.Value.HasValue &&
                                            sensor.Value > 10 &&
                                            sensor.Value < 120 &&
                                            !sensor.Name.ToLower().Contains("fan") &&
                                            !sensor.Name.ToLower().Contains("volt") &&
                                            !sensor.Name.ToLower().Contains("power");

                        if ((isTempByType || isTempByName || isTempByValue) &&
                            sensor.Value.HasValue)
                        {
                            if (!temperatures.ContainsKey(hardware.Name))
                                temperatures[hardware.Name] = new List<Model>();

                            temperatures[hardware.Name].Add(new Model
                            {
                                Name = sensor.Name,
                                Value = sensor.Value.Value,
                                Min = sensor.Min ?? 0,
                                Max = sensor.Max ?? 0
                            });
                        }
                    }
                }

                return temperatures;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения температур: " + ex.Message);
                return null;
            }
        }

    }
}

