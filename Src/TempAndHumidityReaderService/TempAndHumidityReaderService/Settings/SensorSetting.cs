using System;
using System.Text.Json.Serialization;

namespace TempAndHumidityReaderService.Settings
{
    public class SensorSetting
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonPropertyName("GPIO")]
        public int Gpio { get; set; }
        public bool SimulateData { get; set; }
        public int ScanRateMilliSec { get; set; }
        
        [JsonIgnore]
        public bool RemoteRegistered { get; set; }
    }
}