using System;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.DHTxx;
using NLog;
using TempAndHumidityReaderService.Settings;

namespace TempAndHumidityReaderService.Structure
{
    public class Sensor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SensorSetting _setting;
        private readonly SensorData _readData = new();
        private bool _started;
        private ManualResetEventSlim _stopScanning;

        public Sensor(SensorSetting setting)
        {
            _setting = setting ?? throw new ArgumentNullException(nameof(setting));
        }

        public Guid Id => _setting.Id;

        public bool RemoteRegistered => _setting.RemoteRegistered;

        public string Name => _setting.Name;

        public void StartScanning()
        {
            if (_started)
            {
                _logger.Debug($"Sensor '{Name}' data scanning already started");
                return;
            }
            _started = true;
            
            _stopScanning = new ManualResetEventSlim(false);
            Task.Run(() => ScanningData(_setting, _stopScanning, OnScanDataCompleted));
        }

        public void StopScanning()
        {
            if (!_started)
            {
                _logger.Debug($"Sensor '{Name}' data scanning already stopped");
                return;
            }

            _stopScanning?.Set();
            _stopScanning = null;
            _started = false;
        }

        public bool Read(out DateTime readDateTime, out double? temperatureF, out double? humidity)
        {
            bool success;
            
            readDateTime = DateTime.Now;
            temperatureF = null;
            humidity = null;
            
            lock (_readData)
            {
                if (_readData.IsGoodRead)
                {
                    readDateTime = _readData.ReadDateTime;
                    temperatureF = _readData.TemperatureF;
                    humidity = _readData.Humidity;
                }

                success = _readData.IsGoodRead;
            }

            return success;
        }

        private void OnScanDataCompleted(double? temperatureF, double? humidity)
        {
            lock (_readData)
            {
                _readData.TemperatureF = temperatureF;
                _readData.Humidity = humidity;
                _readData.ReadDateTime = DateTime.Now;
            }
        }

        private static void ScanningData(SensorSetting setting, 
            ManualResetEventSlim stopScanning, 
            Action<double?,double?> scanDataCompleted)
        {
            _logger.Debug($"Sensor '{setting.Name}' data scanning started");

            Dht22 dht22 = null;
            Random rand = null;
            
            do
            {
                if (setting.SimulateData)
                {
                    if (rand == null)
                    {
                        rand = new Random();
                        _logger.Debug($"Created random generator for sensor '{setting.Name}'");
                    }
                    GenerateSimulateData(rand, scanDataCompleted);
                }
                else
                {
                    try
                    {
                        if (dht22 == null)
                        {
                            dht22 = new Dht22(setting.Gpio);
                            _logger.Debug($"Created new PIN reader for GPIO '{setting.Gpio}' sensor '{setting.Name}'");
                        }
                        ReadSensorData(dht22, scanDataCompleted);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error scanning data for sensor '{setting.Name}'. ERR: {e}");
                        
                        dht22?.Dispose();
                        dht22 = null;
                    }
                }
            } while (!stopScanning.Wait(setting.ScanRateMilliSec));
            
            dht22?.Dispose();
            _logger.Debug($"Sensor '{setting.Name}' data scanning stopped");
        }

        private static void GenerateSimulateData(Random rand, Action<double?,double?> scanDataCompleted)
        {
            scanDataCompleted?.Invoke(rand.NextDouble(), rand.NextDouble());
        }

        private static void ReadSensorData(DhtBase dht22, Action<double?,double?> scanDataCompleted)
        {
            var tempReader = dht22.Temperature;
            var humidityReader = dht22.Humidity;
            if (dht22.IsLastReadSuccessful)
            {
                var temp = tempReader.DegreesFahrenheit;
                var hum = humidityReader.Percent;

                if (temp >= -50 && temp <= 150 && hum >= 0 && hum <= 100)
                {
                    // Only take good data so that we have something to send to web service.
                    // It may be couple seconds behind but it's better than delaying for another
                    // 10 or 15 seconds.
                    scanDataCompleted?.Invoke(temp, hum);
                }
            }
        }
    }
}