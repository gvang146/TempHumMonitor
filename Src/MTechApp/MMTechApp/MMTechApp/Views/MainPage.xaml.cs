using MMTechApp.Models;
using MMTechApp.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

// TEMPERATURE SERVICE DEPLOYED ON A RASPBERRY PI
// WEB SERVICE IS DEPLOYED A PRIVATE CLOUD
// SENSORS AND MOBILE APPLICATION COMMUNICATES THROUGH THE WEB SERIVCE,
// WEB SERVICE COMMUNICATES WITH DB 
// SENSORS SENDS DATA TO WEB SERVICE, WEB SERIVCE STORES IT IN DB
// MOBILE APP CALLS WEB SERVICE AND THEN IT GETS THE DATA FROM DB AND BRING IT BACK TO MOBILE APP FOR DISPLAY

namespace MMTechApp
{
    public partial class MainPage : ContentPage
    {
        private User _user;
        private IList<Models.Device> _devices = new List<Models.Device>();
        private IList<Models.Sensor> _sensors = new List<Models.Sensor>();
        private SensorData _sensorData;
        public MainPage(User user)
        {
            InitializeComponent();
            this._user = user;
            //DevicePicker.Items.Add("Device_1");
        }
        
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            //var userId = $"http://itestsrv.home.internal:5000/api/device/user/{_user}";
            var getDevice = "http://itestsrv.home.internal:5000/api/device/user";
            var getData = "http://itestsrv.home.internal:5000/api/device/data/1452f13e-8926-4a8f-9f73-cece0ae6f2d0";


            {
                //var userStr = JsonConvert.SerializeObject(getDevice);
                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync($"{getDevice}/{_user.Id}");
                        //IList<Models.Device> devices;
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonContent = await response.Content.ReadAsStringAsync();
                            _devices = JsonConvert.DeserializeObject<List<Models.Device>>(jsonContent);
                        }
                        else
                        {
                            _devices = new List<Models.Device>();
                        }
                        foreach (var device in _devices)
                        {
                            DevicePicker.Items.Add(device.Name);
                        }
                    }
                }
                catch (Exception)
                {
                    await DisplayAlert("device", "Devices not found", "Okay");
                }
            }
        }

        private async void DevicePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var getData = "http://itestsrv.home.internal:5000/api/sensor/device";
           //var selectedValue = picker.Items[picker.SelectedIndex];
            try
            {
                var device = _devices.FirstOrDefault(d => d.Name == DevicePicker.SelectedItem.ToString());

                if (device == null) return;

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"{getData}/{device.Id}");
                    //IList<Models.Sensor> sensors;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        _sensors = JsonConvert.DeserializeObject<List<Models.Sensor>>(jsonContent);
                    }
                    else
                    {
                        _sensors = new List<Models.Sensor>();
                    }
                    foreach (var sensor in _sensors)
                    {
                        SensorPicker.Items.Add(sensor.Name);
                    }
                }
            }
            catch (Exception)
            {
                await DisplayAlert("sensor", "Sensors not found", "Okay");
            }
        }

        private async void SensorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            double tempe;
            double humi;
            var getSensorData = "http://itestsrv.home.internal:5000/api/sensor/data";
            //var selectedValue = picker.Items[picker.SelectedIndex];
            try
            {
                var sensor = _sensors.FirstOrDefault(s => s.Name == SensorPicker.SelectedItem.ToString());

                if (sensor == null) return;

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"{getSensorData}/{sensor.Id}");
                    //IList<Models.Sensor> sensors;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        _sensorData = JsonConvert.DeserializeObject<SensorData>(jsonContent);
                        tempe = _sensorData.Temperature;
                        humi = _sensorData.Humidity;

                        temp.Text = "Temperature: " + tempe;
                        hum.Text = "Humidity: " + humi;

                    }
           
                }
            }
            catch (Exception)
            {
                await DisplayAlert("data", "Datas not found", "Okay");
            }
         
        }
    }

}