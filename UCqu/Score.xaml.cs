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
using CquScoreLib;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Score : Page
    {
        Watcher watcher = null;

        public Score()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter as Watcher != null)
            {
                watcher = e.Parameter as Watcher;
                ScoreSet set = watcher.GetSet((watcher.Workload as SingleWorkload).Workload + "_0");
                NameBox.Text = set.Name;
                UserIdBox.Text = watcher.UserName;
                MajorBox.Text = set.Major;
                GrandGPABox.Text = $"总 GPA: {set.GPA:0.00}";
                MainList.ItemsSource = set;
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(Login), "logout");
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshIconRotation.Begin();
            watcher.Reset();
            ScoreSet set = null;
            await watcher.Perform();
            set = watcher.GetSet((watcher.Workload as SingleWorkload).Workload + "_0");
            NameBox.Text = set.Name;
            UserIdBox.Text = watcher.UserName;
            MajorBox.Text = set.Major;
            GrandGPABox.Text = $"总 GPA: {set.GPA:0.00}";
            MainList.ItemsSource = set;
            RefreshIconRotation.Stop();
        }

        private async void SecondSwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SecondSwitchBtn.IsChecked == true)
            {
                ScoreSet set = watcher.GetSet((watcher.Workload as SingleWorkload).Workload + "_1");
                if (set == null)
                {
                    SecondSwitchBtn.IsChecked = false;
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "无辅修数据";
                    dialog.Content = "未找到任何辅修成绩";
                    dialog.CloseButtonText = "确定";
                    await dialog.ShowAsync();
                }
                else
                {
                    MainList.ItemsSource = set;
                    NameBox.Text = set.Name;
                    UserIdBox.Text = watcher.UserName;
                    MajorBox.Text = set.Major;
                    GrandGPABox.Text = $"总 GPA: {set.GPA:0.00}";
                }
            }
            else
            {
                ScoreSet set = watcher.GetSet((watcher.Workload as SingleWorkload).Workload + "_0");
                if (set == null) { }
                else
                {
                    MainList.ItemsSource = set;
                    NameBox.Text = set.Name;
                    UserIdBox.Text = watcher.UserName;
                    MajorBox.Text = set.Major;
                    GrandGPABox.Text = $"总 GPA: {set.GPA:0.00}";
                }
            }
        }

        private async void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "UCQU Alpha 1 by David Lee";
            dialog.Content = "这是该应用程序的早期预览版。\n未经作者授权，您不得对该应用程序进行任何形式的再发行或逆向工程。\n\n" +
                "免责声明: \n本应用程序与重庆大学官方无任何关联，亦未经重庆大学官方认可；应用程序作者与重庆大学官方无商业或技术联系。" +
                "本应用程序按 \"原样\" 提供。应用程序的问题若为用户带来任何损失，应用程序作者不负任何直接或连带责任，用户应自行权衡，决定是否使用任何应用程序功能。\n\n" +
                "隐私声明 (最后更新时间 2018/7/16):\n默认设置下，本应用程序不对任何用户数据执行永久化存储。若用户登录时选择保持登录状态，则应用程序将在设备本地存储用户的\n" +
                "a) 教务网登录用户名; b) 以教务平台规范方法加密的密码哈希; c) 用户所选择的登录服务器.\n用户可以随时通过退出登录来清除这些数据。";
            dialog.CloseButtonText = "确定";
            await dialog.ShowAsync();
        }
    }

    public class GPAStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double)value - 1;
        }
    }
}
