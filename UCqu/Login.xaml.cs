﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CquScoreLib;
using System.Threading.Tasks;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadingRingGrid.Visibility = Visibility.Visible;
            LoadingRing.IsActive = true;
            string pwdHash = Watcher.GetPasswordHash(IdBox.Text, PswBox.Password);
            string host = (HostBox.SelectedItem as ComboBoxItem).Content as string;
            Watcher watcher = new Watcher(host, IdBox.Text, pwdHash, 0);
            await watcher.LoginAsync();
            bool isCorrect = await watcher.ValidateLoginAsync();
            if (isCorrect)
            {
                watcher.Workload = new SingleWorkload(IdBox.Text);
                await watcher.Perform();
                if(SavePwdBox.IsChecked == true)
                {
                    SaveCredentials(IdBox.Text, pwdHash, host);
                }
                LoadingRing.IsActive = false;
                LoadingRingGrid.Visibility = Visibility.Collapsed;
                (Window.Current.Content as Frame).Navigate(typeof(Score), watcher);
            }
            else
            {
                PwdIncorrectHint.Visibility = Visibility.Visible;
                LoadingRing.IsActive = false;
                LoadingRingGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void PswBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginBtn_Click(this, null);
            }
        }

        private void IdBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginBtn_Click(this, null);
            }
        }

        void SaveCredentials(string id, string pwdHash, string host)
        {
            SaveCredentials(id, pwdHash, host, false);
        }
        void SaveCredentials(string id, string pwdHash, string host, bool clear)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (!clear)
            {
                localSettings.Values["id"] = id;
                localSettings.Values["pwdHash"] = pwdHash;
                localSettings.Values["host"] = host;
            }
            else
            {
                localSettings.Values["id"] = null;
                localSettings.Values["pwdHash"] = null;
                localSettings.Values["host"] = null;
            }
        }
        bool LoadCredentials(out string id, out string pwdHash, out string host)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string uid = localSettings.Values["id"] as string;
            if (uid == null) { id = ""; pwdHash = ""; host = ""; return false; }
            else
            {
                id = uid; pwdHash = localSettings.Values["pwdHash"] as string; host = localSettings.Values["host"] as string;
                return true;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.Parameter != null)
            {
                if(e.Parameter as string != "")
                {
                    SaveCredentials("", "", "",true);
                    return;
                }
            }

            if (LoadCredentials(out string id, out string pwdHash, out string host) == true)
            {
                LoadingRingGrid.Visibility = Visibility.Visible;
                LoadingRing.IsActive = true;
                Watcher watcher = new Watcher(host, id, pwdHash, 0);
                await watcher.LoginAsync();
                bool isCorrect = await watcher.ValidateLoginAsync();
                if (isCorrect)
                {
                    watcher.Workload = new SingleWorkload(id);
                    await watcher.Perform();
                    LoadingRing.IsActive = false;
                    LoadingRingGrid.Visibility = Visibility.Collapsed;
                    (Window.Current.Content as Frame).Navigate(typeof(Score), watcher);
                }
                else
                {
                    IdBox.Text = id;
                    SavePwdBox.IsChecked = false;
                    PwdIncorrectHint.Visibility = Visibility.Visible;
                    LoadingRing.IsActive = false;
                    LoadingRingGrid.Visibility = Visibility.Collapsed;
                }

            }
        }
    }
}