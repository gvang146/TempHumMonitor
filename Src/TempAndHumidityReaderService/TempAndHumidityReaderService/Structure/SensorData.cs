using System;

namespace TempAndHumidityReaderService.Structure
{
    public class SensorData
    {
        public SensorData()
        {
            ReadDateTime = DateTime.Now;
        }

        public bool IsGoodRead => TemperatureF.HasValue && Humidity.HasValue;
        
        public DateTime ReadDateTime { get; set; }
        public double? TemperatureF { get; set; }
        public double? Humidity { get; set; }
    }
}