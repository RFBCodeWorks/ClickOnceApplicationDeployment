using System;
using System.Diagnostics;
using System.Windows;
using NLog;
using RFBApplicationDeployment;

namespace WpfSettings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        private static ClickOnceApplicationDeployment EntryApp => ClickOnceApplicationDeployment.EntryApplication;
        private ClickOnceApplicationDeployment _ClickOnce;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ClickOnceApplicationDeployment.SetupEntryApplication("http://test:8080/clickonce/");
            _ClickOnce = EntryApp;
        }

        private void ButtonIsNetworkInstalled_OnClick(object sender, RoutedEventArgs e)
        {
            LabelIsNetworkInstalled.Content = _ClickOnce.IsNetworkDeployed;
        }

        private void ButtonLocalVersion_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LabelLocalVersion.Content = _ClickOnce.CurrentVersion;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private async void ButtonGetServerVersion_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var res = await _ClickOnce.CheckForDetailedUpdateAsync();
                string str = $" Network Version: {res.AvailableVersion} \n Check Successful: {!res.Cancelled} \n Error Data: {(res.Error.Message ?? "No Errors Occurred")}";
                LabelServerVersion.Content = str;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private async void ButtonUpdateAvailable_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LabelUpdateAvailable.Content = await _ClickOnce.CheckUpdateAvailableAsync();
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                LabelUpdateAvailable.Content = exp.Message;
            }
        }

        private async void ButtonUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // On update don't make application restart! This is important!
                // Just make shutdown of current app
                var updateResult = await _ClickOnce.UpdateAsync();
                LabelUpdate.Content = updateResult;
                if (updateResult)
                {
                    Application.Current.Shutdown(0);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private void ButtonDataDir_OnClick(object sender, RoutedEventArgs e)
        {
            LabelDataDir.Content = _ClickOnce.DataDirectory;
        }
    }
}
