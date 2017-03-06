using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Clipboard__
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Deactivated(object sender, EventArgs e) {
            var windows = Application.Current.Windows;

            foreach (Window window in windows) {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
