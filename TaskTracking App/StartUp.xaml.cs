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

namespace TaskTracking_App
{
    public partial class StartUp : Page
    {
        public StartUp()
        {
            InitializeComponent();
        }

        public void SwitchToSignUp(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }

        public void SwitchToSignIn(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new SignIn());
        }
    }
}
