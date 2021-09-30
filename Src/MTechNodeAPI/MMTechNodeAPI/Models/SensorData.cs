using System;

namespace MMTechNodeAPI.Models
{
    public class SensorData
    {
        public Guid Id { get; set; }
        public Guid SensorId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime TimeRecord { get; set; }
    }
}