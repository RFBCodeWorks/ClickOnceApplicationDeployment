﻿using System;
using System.IO;
using System.Reflection;
using System.Windows;
using NLog;

namespace WpfSettings
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        protected override void OnStartup(StartupEventArgs e)
        {

            RFBApplicationDeployment.ClickOnceApplicationDeployment.SetupEntryApplication(@"C:\TestPublishPath\");
            base.OnStartup(e);
            this.MainWindow = new WpfSettings.MainWindow();
            this.MainWindow.Show();

        }

        public App()
        {
            var logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();

            try
            {
                var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                logger.Info($"Started. {execDir}");
            }
            catch (Exception exp)
            {
                logger.Error(exp, exp.Message);
            }
        }
    }
}
