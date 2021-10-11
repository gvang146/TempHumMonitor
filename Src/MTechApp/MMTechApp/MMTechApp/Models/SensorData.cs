using System;
using System.Collections.Generic;
using System.Text;

namespace MMTechApp.Models
{   // sensor data for storage
    class SensorData
    {
        public Guid Id { get; set; }
        public Guid SensorId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime TimeRecord { get; set; }
    }
}
