using System;
using System.Collections.Generic;

namespace TempAndHumidityReaderService.Settings
{
    public class DeviceSetting
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ReadingRateSec { get; set; }
        public IList<SensorSetting> Sensors { get; set; }
        public string WebApi { get; set; }
    }
}