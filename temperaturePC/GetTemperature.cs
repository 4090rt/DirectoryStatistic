using OpenHardwareMonitor.Hardware;
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
            _computer.Open(true);
        }

        public static Dictionary<string, List<Model>> Getemp()
        {
            try
            {
                var temperatrure = new Dictionary<string, List<Model>>();

                if (_computer == null)
                {
                    Initialize();
                }

                foreach (var hardware in _computer.Hardware)
                {
                    hardware.Update();

                    foreach (var sensors in hardware.Sensors)
                    {
                        if (sensors.SensorType == SensorType.Temperature && sensors.Value.HasValue)
                        {
                            if (!temperatrure.ContainsKey(hardware.Name))
                            {
                                temperatrure[hardware.Name] = new List<Model>();
                            }

                            temperatrure[hardware.Name].Add(new Model
                            {
                                Name = hardware.Name,
                                Value = sensors.Value.Value,
                                Min = sensors.Min.Value,
                                Max = sensors.Max.Value
                            });
                        }
                    }
                }
                return temperatrure;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Не получилось получить температуру  датчиков" + ex.Message);
                return null;
            }
        }
    }
}
