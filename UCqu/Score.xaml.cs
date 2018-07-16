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
