using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;
using static TaskTracking_App.SignIn;

namespace TaskTracking_App
{
    public partial class SignUp : Page
    {
        public SignUp()
        {
            InitializeComponent();
            username.Text = "Username...";
            username.GotFocus += (s, e) => {
                if (username.Text == "Username...")
                {
                    username.Text = "";
                }
            };
            username.LostFocus += (s, e) => {
                if (username.Text == "")
                {
                    username.Text = "Username...";
                }
            };

            email.Text = "Email...";
            email.GotFocus += (s, e) => {
                if (email.Text == "Email...")
                {
                    email.Text = "";
                }
            };
            email.LostFocus += (s, e) => {
                if (email.Text == "")
                {
                    email.Text = "Email...";
                }
            };

            password.Text = "Password...";
            password.GotFocus += (s, e) => {
                if (password.Text == "Password...")
                {
                    password.Text = "";
                }
            };
            password.LostFocus += (s, e) => {
                if (password.Text == "")
                {
                    password.Text = "Password...";
                }
            };
        }

        public void SwitchToStart(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartUp());
        }

        public void TrySignUp(object sender, RoutedEventArgs e)
        {
            string tryUsername = username.Text;
            string tryEmail = email.Text;
            string tryPassword = password.Text;

            if (tryUsername == "Username..." || tryEmail == "Email..." || tryPassword == "Password...")
            {
                MainSignUpLabel.Content = "Fill All Fields";
                MainSignUpLabel.Foreground = new SolidColorBrush(Colors.Red);
                MainSignUpLabel.Width = 200;
                MainSignUpLabel.Margin = new Thickness(0, 0, 50, 0);
                return;
            }
            if (tryUsername.Contains(" ") || tryEmail.Contains(" ") || tryPassword.Contains(" "))
            {
                MainSignUpLabel.Content = "No Spaces Allowed";
                MainSignUpLabel.Foreground = new SolidColorBrush(Colors.Red);
                MainSignUpLabel.Width = 200;
                MainSignUpLabel.Margin = new Thickness(0, 0, 50, 0);
                return;
            }
            if (tryPassword.Length < 3)
            {
                MainSignUpLabel.Content = "Password Is Too Short";
                MainSignUpLabel.Foreground = new SolidColorBrush(Colors.Red);
                MainSignUpLabel.Width = 250;
                MainSignUpLabel.Margin = new Thickness(0, 0, 0, 0);
                return;
            }


            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Sign up");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { tryUsername, tryEmail, tryPassword};
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += Ws_OnMessage;
                }
                else
                {
                    MainSignUpLabel.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                MainSignUpLabel.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var receivedDataList = e.Data; // assuming e.Data contains the received serialized JSON string
                var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedDataList);
                if (deserializedDataList[0].ToString() == "True")
                {
                    NavigationService.Navigate(new MainTracker(deserializedDataList[1].ToString()));
                }
                else if (deserializedDataList[0].ToString() == "False")
                {
                    MainSignUpLabel.Content = "Username Taken";
                    MainSignUpLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MainSignUpLabel.Width = 200;
                    MainSignUpLabel.Margin = new Thickness(0, 0, 50, 0);
                }
            });
        }
    }
}
