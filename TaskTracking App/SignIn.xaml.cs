using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
using Newtonsoft.Json;
using WebSocketSharp;



namespace TaskTracking_App
{
    public partial class SignIn : Page
    {
        public SignIn()
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

        public void TrySignIn(object sender, RoutedEventArgs e)
        {
            if (username.Text == "Username...")
            {
                MainSignInLabel.Content = "Username empty";
                MainSignInLabel.Foreground = new SolidColorBrush(Colors.Red);
                MainSignInLabel.Width = 250;
                MainSignInLabel.Margin = new Thickness(0, 0, 0, 0);
                return;
            }
            if (password.Text == "Password...")
            {
                MainSignInLabel.Content = "Password empty";
                MainSignInLabel.Foreground = new SolidColorBrush(Colors.Red);
                MainSignInLabel.Width = 250;
                MainSignInLabel.Margin = new Thickness(0, 0, 0, 0);
                return;
            }
            if (username.Text.Length > 0 && password.Text.Length < 3)
            {
                MainSignInLabel.Content = "Password Is Too Short";
                MainSignInLabel.Foreground = new SolidColorBrush(Colors.Red);
                MainSignInLabel.Width = 250;
                MainSignInLabel.Margin = new Thickness(0, 0, 0, 0);
                return;
            }

            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Sign in");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { username.Text, password.Text };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += Ws_OnMessage;
                }
                else
                {
                    MainSignInLabel.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                MainSignInLabel.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
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
                    MainSignInLabel.Content = "Invalid Credentials";
                    MainSignInLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MainSignInLabel.Width = 200;
                    MainSignInLabel.Margin = new Thickness(0, 0, 50, 0);
                }
            });
        }
    }
}
