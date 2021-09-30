using MMTechApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MMTechApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void LoginProcedure(object sender, EventArgs e)
        {
            //assigning entered user and pass to local for identification
            User user = new User(enUsername.Text,enPassword.Text);
            //sends the user through to check if the user and pass are filled in 
            if (user.CheckInformation())
            {
                //assigning WebAPI url
                var loginUrl = "http://itestsrv.home.internal:5000/api/user/login";
                //try catch block to cycle through the information
                try
                {
                    using (var client = new HttpClient()) //assigning new HttpClient to local
                    {
                        var userStr = JsonConvert.SerializeObject(user); //serializing the user from http to json
                        var sContentType = "application/json"; 
                        //httppost to request data from webapi
                        var response = await client.PostAsync(loginUrl, new StringContent(userStr, Encoding.UTF8, sContentType));
                        if (response.IsSuccessStatusCode) //checking to see if user and pass exists in webapi database
                        {
                            //assigning our local response var with the obtained one from webapi
                            var responseContent = await response.Content.ReadAsStringAsync();
                            //responseContent = responseContent.Trim('\"'); //trimming the return ID of our account?
                            //user.Id = new Guid(responseContent); //assignign the obtained id to our local userId
                            user = JsonConvert.DeserializeObject<User>(responseContent);
                        }
                    }
                } catch(Exception ex)
                {
                    await DisplayAlert("Login", $"Error logging in: {ex.Message}", "OK");
                    return;
                }
            }

            if (user.Id != Guid.Empty)
            {
                //await DisplayAlert("Login", "Login Success", "Okay");
                await Navigation.PushAsync(new MainPage(user));
            }
            else
            {
                await DisplayAlert("Login", "Invalid Username or Password", "Okay");
            }
        }
    }
}