using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Presentation.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(String.Format("\r\nException Message:{0}\r\nException StackTrace:{1}\r\n", e.Exception.Message.ToString(), e.Exception.StackTrace.ToString()), "UnhandledExceptionEventArgs");
            e.Handled = true;
        }
    }
}
