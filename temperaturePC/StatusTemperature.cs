using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.temperaturePC
{
    public class StatusTemperature
    {
        public static string GetTemperatureStatus(float temp, string sensorName)
        {
            if (sensorName.Contains("CPU") || sensorName.Contains("Core"))
            {
                if (temp < 50) return "✓ Норма";
                if (temp < 70) return "⚠  рабочая температура под нагрузкой";
                if (temp < 85) return "[▲ Предел безопасной рабочей температуры";

                return ("🔥 Критично!");
            }

            if (sensorName.Contains("GPU"))
            {
                if (temp < 60) return "✓ Норма";
                if (temp < 80) return "⚠  рабочая температура под нагрузкой";
                if (temp < 95) return "[▲ Предел безопасной рабочей температуры";

                return ("🔥 Критично!");
            }

            else if (sensorName.Contains("SSD") || sensorName.Contains("HDD"))
            {
                if (temp < 45) return "[✓ Норма]";
                if (temp < 55) return "[⚠ Тепло]";
                if (temp < 65) return "[▲ Горячо]";
                return "[🔥 Критично!]";
            }

            return temp < 60 ? "[✓ Норма]" : "[⚠ Повышена]";
        }
    }
}
