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
using Windows.Foundation.Metadata;
using System.Net;
using Controls = Microsoft.UI.Xaml.Controls;
using Model = DL444.UcquLibrary.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Score : Page
    {
        string token;
        Model.Score majorScore;
        Model.Score secondMajorScore;

        bool secondMajor = false;

        public Score()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ScorePageNavigationParameter p)
            {
                majorScore = p.MajorScore;
                PopulateList(majorScore);
                token = p.Token;
                try
                {
                    secondMajorScore = await WebClient.GetScoreAsync(token, false);
                }
                catch (WebException) { }
                catch (RequestFailedException) { }
            }
        }

        private void PopulateList(Model.Score score)
        {
            score.Terms.Sort();
            score.Terms.Reverse();
            MainList.ItemsSource = score.Terms;
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshIconRotation.Begin();
            await Refresh();
            RefreshIconRotation.Stop();
        }

        private async System.Threading.Tasks.Task Refresh()
        {
            if(!secondMajor)
            {
                try
                {
                    majorScore = await WebClient.GetScoreAsync(token);
                    PopulateList(majorScore);
                }
                catch(WebException)
                {
                    RefreshFailedNotification.Show("刷新失败, 请检查网络连接", 5000);
                }
                catch(RequestFailedException ex)
                {
                    RefreshFailedNotification.Show($"服务器未知错误，请稍后再试 (2.{ex.Status})", 5000);
                }
            }
            else
            {
                try
                {
                    secondMajorScore = await WebClient.GetScoreAsync(token, false);
                    PopulateList(secondMajorScore);
                }
                catch (WebException)
                {
                    RefreshFailedNotification.Show("刷新失败, 请检查网络连接", 5000);
                }
                catch (RequestFailedException ex)
                {
                    RefreshFailedNotification.Show($"服务器未知错误，请稍后再试 (2.{ex.Status})", 5000);
                }
            }
        }

        private async void SecondSwitchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SecondSwitchBtn.IsChecked == true)
            {
                if (secondMajorScore == null)
                {
                    try
                    {
                        secondMajorScore = await WebClient.GetScoreAsync(token, false);
                    }
                    catch (WebException)
                    {
                        SecondSwitchBtn.IsChecked = false;
                        RefreshFailedNotification.Show("刷新失败, 请检查网络连接", 5000);
                    }
                    catch (RequestFailedException ex)
                    {
                        SecondSwitchBtn.IsChecked = false;
                        RefreshFailedNotification.Show($"服务器未知错误，请稍后再试 (2.{ex.Status})", 5000);
                    }

                    if(secondMajorScore.Terms.Count == 0)
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
                        secondMajor = true;
                        PopulateList(secondMajorScore);
                    }
                }
                else
                {
                    if(secondMajorScore.Terms.Count == 0)
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
                        secondMajor = true;
                        PopulateList(secondMajorScore);
                    }
                }
            }
            else
            {
                secondMajor = false;
                PopulateList(majorScore);
            }
        }

        private async void RefreshContainer_RefreshRequested(Controls.RefreshContainer sender, Controls.RefreshRequestedEventArgs args)
        {
            using (var RefreshCompletionDeferral = args.GetDeferral())
            {
                await Refresh();
            }
        }
    }

    class ScorePageNavigationParameter
    {
        public ScorePageNavigationParameter(string token, Model.Score majorScore)
        {
            Token = token;
            MajorScore = majorScore;
        }

        public string Token { get; set; }
        public Model.Score MajorScore { get; set; }
    }

    public class GPAStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if((double)value < 1.0) { return -1.0; }
            return (double)value / 4.0 * 5.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if((double)value == -1.0) { return 0.0; }
            return (double)value * 4.0 / 5.0;
        }
    }
    public class GrandGPAConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"总绩点: {(double)value:0.00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
    public class TermGPAConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"学期绩点: {(double)value:0.00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
    public class CourseGPAConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{(double)value:0.0}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }

}
