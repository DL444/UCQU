using System;
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
using System.Threading.Tasks;
using Windows.UI.Core;
using System.Net;
using Model = DL444.UcquLibrary.Models;
using System.Collections.Immutable;

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
            if(PswBox.Password == "" || IdBox.Text == "")
            {
                LoginFailedNotification.Show("请输入用户名及密码", 5000);
                return;
            }
            string pwdHash = GetPasswordHash(IdBox.Text, PswBox.Password);
            string userId = IdBox.Text;

            await LoginAsync(userId, pwdHash);
        }

        private async Task LoginAsync(string userId, string pwdHash)
        {
            LoadingRingGrid.Visibility = Visibility.Visible;
            LoadingRing.IsActive = true;

            string token;
            try
            {
                token = await WebClient.LoginAsync(userId, pwdHash);
            }
            catch (WebException)
            {
                ShowErrorMessage("数据获取失败, 请检查网络连接");
                return;
            }

            if (token.Length > 1)
            {
                if (RuntimeData.LoadSetting("courseToastSwitch", out string _cSwitch) == false) { RuntimeData.SaveSetting("courseToastSwitch", "on"); }
                if (RuntimeData.LoadSetting("dailyToastSwitch", out string _dSwitch) == false) { RuntimeData.SaveSetting("dailyToastSwitch", "on"); }
                if (RuntimeData.LoadSetting("imgToastSwitch", out string _switch) == false) { RuntimeData.SaveSetting("imgToastSwitch", "on"); }
                SaveCredentials(userId, pwdHash);

                RuntimeData.Token = token;
                RuntimeData.UserId = userId;

                // Get static data.
                try
                {
                    Model.StaticDataModel staticData = await WebClient.GetStaticDataAsync();
                    RuntimeData.StartDate = staticData.StartDate;
                    RuntimeData.StartTimeABC = ImmutableArray.Create(staticData.StartTimeABC.ToArray());
                    RuntimeData.StartTimeD = ImmutableArray.Create(staticData.StartTimeD.ToArray());
                    RuntimeData.EndTimeABC = ImmutableArray.Create(staticData.EndTimeABC.ToArray());
                    RuntimeData.EndTimeD = ImmutableArray.Create(staticData.EndTimeD.ToArray());
                }
                catch (WebException)
                {
                    ShowErrorMessage("数据获取失败, 请检查网络连接");
                    return;
                }

                // Get student info.
                try
                {
                    RuntimeData.StudentInfo = await WebClient.GetStudentInfoAsync(token);
                }
                catch (WebException)
                {
                    ShowErrorMessage("数据获取失败, 请检查网络连接");
                    return;
                }
                catch (RequestFailedException ex)
                {
                    ShowErrorMessage($"服务器未知错误，请稍后再试 (1.{ex.Status})");
                    return;
                }

                // Get score.
                try
                {
                    RuntimeData.Score = await WebClient.GetScoreAsync(token);
                }
                catch (WebException)
                {
                    ShowErrorMessage("数据获取失败, 请检查网络连接");
                    return;
                }
                catch (RequestFailedException ex)
                {
                    ShowErrorMessage($"服务器未知错误，请稍后再试 (2.{ex.Status})");
                    return;
                }

                // Get schedule.
                try
                {
                    RuntimeData.Schedule = await WebClient.GetScheduleAsync(token);
                }
                catch (WebException)
                {
                    ShowErrorMessage("数据获取失败, 请检查网络连接");
                    return;
                }
                catch (RequestFailedException ex)
                {
                    ShowErrorMessage($"服务器未知错误，请稍后再试 (3.{ex.Status})");
                    return;
                }

                LoadingRing.IsActive = false;
                LoadingRingGrid.Visibility = Visibility.Collapsed;
                (Window.Current.Content as Frame).Navigate(typeof(MainPage));
            }
            else if (token == "1")
            {
                ShowErrorMessage("用户名与密码不匹配, 请重试");
                return;
            }
            else if (token == "4")
            {
                ShowErrorMessage("教务系统尚未开放，请确认已缴纳学费后等待教务系统开放");
                return;
            }
            else
            {
                ShowErrorMessage($"服务器未知错误，请稍后再试 (0.{token})");
                return;
            }

            void ShowErrorMessage(string message)
            {
                LoginFailedNotification.Show(message, 5000);
                LoadingRing.IsActive = false;
                LoadingRingGrid.Visibility = Visibility.Collapsed;
            }
        }


        private void Enter_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginBtn_Click(this, null);
            }
        }

        void SaveCredentials(string id, string pwdHash)
        {
            SaveCredentials(id, pwdHash, false);
        }
        void SaveCredentials(string id, string pwdHash, bool clear)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (!clear)
            {
                localSettings.Values["id"] = id;
                localSettings.Values["pwdHash"] = pwdHash;
            }
            else
            {
                localSettings.Values["id"] = null;
                localSettings.Values["pwdHash"] = null;
            }
        }
        internal static bool LoadCredentials(out string id, out string pwdHash)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string uid = localSettings.Values["id"] as string;
            if (uid == null) { id = ""; pwdHash = ""; return false; }
            else
            {
                id = uid; pwdHash = localSettings.Values["pwdHash"] as string;
                return true;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.Parameter != null)
            {
                if(e.Parameter as string == "logout")
                {
                    SaveCredentials("", "", true);
                    RuntimeData.SaveSetting("campus", null);
                    return;
                }
            }

            if (LoadCredentials(out string id, out string pwdHash) == true)
            {
                await LoginAsync(id, pwdHash);
            }
        }

        public static string GetPasswordHash(string userId, string pwd)
        {
            string schoolCode = "10611";
            string encrypted = "";
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] pwdArray = System.Text.Encoding.ASCII.GetBytes(pwd);
            byte[] hash = md5.ComputeHash(pwdArray);
            string hashStr = "";
            foreach (byte b in hash)
            {
                hashStr += b.ToString("X2");
            }
            hashStr = userId + hashStr.Substring(0, 30).ToUpper() + schoolCode;
            pwdArray = System.Text.Encoding.ASCII.GetBytes(hashStr);
            hash = md5.ComputeHash(pwdArray);
            hashStr = "";
            foreach (byte b in hash)
            {
                hashStr += b.ToString("X2");
            }
            encrypted = hashStr.Substring(0, 30).ToUpper();
            return encrypted;
        }

    }
}
