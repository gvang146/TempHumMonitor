using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using TempAndHumidityReaderService.Settings;

namespace TempAndHumidityReaderService.Structure
{
    public class Device
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly DeviceSetting _setting;
        private readonly IList<Task> _dataReaderTasks = new List<Task>();
        
        private bool _started;
        private ManualResetEventSlim _stopMonitoring;

        public Device(DeviceSetting setting)
        {
            _setting = setting ?? throw new ArgumentNullException(nameof(setting));
        }

        public void ConnectToWebApi()
        {
            if (string.IsNullOrEmpty(_setting.WebApi))
            {
                _logger.Warn("Server URL was not set. Data will not be pushed to server");
                return;
            }
            
            _logger.Debug($"Server URL: {_setting.WebApi}");
            
            try
            {
                var baseUrl = _setting.WebApi;
                using var client = new HttpClient();
                
                var response = client.GetAsync($"{baseUrl}/device/{_setting.Id}").GetAwaiter().GetResult();
                
                // Device can no longer register itself with server. A mobile or web app must do the binding.
                // That's because device needs to be tie to a user account.
                /*
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.Debug($"Could not find device '{_setting.Name}' on server. Will register as new device");
                    response = client.PostAsJsonAsync($"{baseUrl}/device", new
                    {
                        Id = _setting.Id,
                        Name = _setting.Name,
                        Description = _setting.Description
                    }).GetAwaiter().GetResult();
                    _logger.Debug($"Registered device '{_setting.Name}' successfull");
                }
                else if (response.IsSuccessStatusCode)
                {
                    _logger.Debug($"Device '{_setting.Name}' was already registered");
                }
                */

                if (response.IsSuccessStatusCode)
                {
                    _logger.Debug("Checking to see if sensors need to be registered");
                    foreach (var sensor in _setting.Sensors)
                    {
                        response = client.GetAsync($"{baseUrl}/sensor/{sensor.Id}").GetAwaiter().GetResult();
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            response = client.PostAsJsonAsync($"{baseUrl}/sensor", new
                            {
                                Id = sensor.Id,
                                DeviceId = _setting.Id,
                                Name = sensor.Name,
                                Description = sensor.Description
                            }).GetAwaiter().GetResult();
                            _logger.Debug($"Registered sensor '{sensor.Name}' successfull");
                        }
                        else if (response.IsSuccessStatusCode)
                        {
                            _logger.Debug($"Sensor '{sensor.Name}' was already registerred");
                        }

                        sensor.RemoteRegistered = response.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception e)
            {
                var errMsg = $"Error checking or registering device and sensors. ERR: {e}";
                _logger.Error(errMsg);
                Console.WriteLine(errMsg);
            }
        }

        public void StartMonitoring()
        {
            if (_started)
            {
                _logger.Debug("Device monitoring was already started");
                return;
            }
            _started = true;

            _stopMonitoring = new ManualResetEventSlim(false);
            
            foreach (var sensorSetting in _setting.Sensors)
            {
                var sensor = new Sensor(sensorSetting);
                _dataReaderTasks.Add(Task.Run(() => DataReaderTask(sensor, _setting, _stopMonitoring)));
            }
        }

        public void StopMonitoring()
        {
            if (!_started)
            {
                _logger.Debug($"Device monitoring was already stopped");
                return;
            }
            _stopMonitoring?.Set();
            _stopMonitoring = null;

            Task.WaitAll(_dataReaderTasks.ToArray());

            _started = false;
        }

        private static void DataReaderTask(Sensor sensor, DeviceSetting deviceSetting, ManualResetEventSlim stopMonitoring)
        {
            _logger.Debug($"Device monitoring started for sensor '{sensor.Name}'");
            
            var baseUrl = $"{deviceSetting.WebApi}/sensordata";
            var client = new HttpClient();

            var lastDataRead = DateTime.Now.AddSeconds(-1);
            
            sensor.StartScanning();

            var delayInMilliseconds = deviceSetting.ReadingRateSec * 1000;
            do
            {
                try
                {
                    if (sensor.Read(out var readDateTime, out var tempF, out var humidity))
                    {
                        _logger.Trace($"Read data from sensor '{sensor.Name}', Time: {readDateTime}, TempF: {tempF}, Humidity: {humidity}");
                        if (readDateTime > lastDataRead)
                        {
                            lastDataRead = readDateTime;
                            
                            if (sensor.RemoteRegistered)
                            {
                                client.PostAsJsonAsync(baseUrl, new
                                {
                                    SensorId = sensor.Id,
                                    Temperature = tempF,
                                    Humidity = humidity,
                                    TimeRecord = readDateTime
                                }).GetAwaiter().GetResult();
                                _logger.Trace($"Finished pushing sensor '{sensor.Name}' data to server");
                            }
                            else
                            {
                                _logger.Trace($"Sensor was not reigstered with server, will not push sensor '{sensor.Name}' data to server");
                            }
                        }
                        else
                        {
                            _logger.Trace($"Data is same as previous, will not push sensor '{sensor.Name}' data to server");
                        }
                    }
                    else
                    {
                        _logger.Trace($"Bad reading, will not push sensor '{sensor.Name}' data to server");
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Error reading data from sensor and pushing data to server. ERR: {e}");
                }
            } while (!stopMonitoring.Wait(delayInMilliseconds));
            
            sensor.StopScanning();
            
            _logger.Debug($"Device monitoring has stopped for sensor '{sensor.Name}'");
        }
    }
}