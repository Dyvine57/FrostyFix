﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FrostyFix.Dialogs;
using FrostyFix.SettingsManager;
using PropertyChanged;
using Newtonsoft.Json;

namespace FrostyFix {

    [AddINotifyPropertyChangedInterface]
    public class Config : SettingsManager<Config> {
        public bool LaunchGame { get; set; } = false;
        public bool BackgroundThread { get; set; } = true;
        public bool UpdateChecker { get; set; } = true;

        public string FrostyPath { get; set; }
        public string CustomGamePath { get; set; }

        public int SelectedGame { get; set; } = -1;
        public int SelectedPlatform { get; set; } = -1;
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public static readonly string Version = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString()[..5];
        public static readonly string AppName = Assembly.GetEntryAssembly().GetName().Name;

        public App() {
            Config.Load();

            if (Config.Settings.UpdateChecker) 
                CheckVersion("Dyvinia", "FrostyFix");

            DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;
            string title = "FrostyFix";
            ExceptionDialog.Show(e.Exception, title, true);
        }

        public async void CheckVersion(string repoAuthor, string repoName) {
            try {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "request");

                dynamic json = JsonConvert.DeserializeObject<dynamic>(await client.GetStringAsync($"https://api.github.com/repos/{repoAuthor}/{repoName}/releases/latest"));
                Version latest = new(((string)json.tag_name)[1..]);
                Version local = Assembly.GetExecutingAssembly().GetName().Version;

                if (local.CompareTo(latest) < 0) {
                    string message = $"You are using {AppName} v{local.ToString()[..5]}. \nWould you like to download the latest version? (v{latest})";
                    MessageBoxResult Result = MessageBoxDialog.Show(message, "FrostyFix", MessageBoxButton.YesNo, DialogSound.Notify);
                    if (Result == MessageBoxResult.Yes) {
                        Process.Start(new ProcessStartInfo($"https://github.com/{repoAuthor}/{repoName}/releases/latest") { UseShellExecute = true });
                    }
                }
            }
            catch (Exception e) {
                ExceptionDialog.Show(e, AppName, false, "Unable to check for updates:");
            }
        }
    }
}
