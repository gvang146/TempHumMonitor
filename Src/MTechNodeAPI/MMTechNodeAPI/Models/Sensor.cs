using System;

namespace MMTechNodeAPI.Models
{
    public class Sensor
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}