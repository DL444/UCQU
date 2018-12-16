using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NewControls = Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using Model = DL444.UcquLibrary.Models;
using System.ComponentModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string userId;
        string token;
        Model.StudentInfo info;
        Model.Score score;
        Model.Schedule schedule;

        public MainPage()
        {
            this.InitializeComponent();

            foreach (NewControls.NavigationViewItem i in NavPane.MenuItems)
            {
                if (i.Tag.ToString() == "Home")
                {
                    NavPane.SelectedItem = i;
                    break;
                }
            }
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            NavPane.SelectedItem = null;

            if (!(ContentFrame.Content is About))
            {
                ContentFrame.Navigate(typeof(About));
            }
        }

        private void NavPane_SelectionChanged(NewControls.NavigationView sender, NewControls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == null)
            {
                HeaderControl.Visibility = Visibility.Collapsed;
                return;
            }

            string selectionTag = (args.SelectedItem as NewControls.NavigationViewItem).Tag.ToString();

            HeaderControl.Visibility = selectionTag == "Home" ? Visibility.Visible : Visibility.Collapsed;
            switch (selectionTag)
            {
                case "Home":
                    if (!(ContentFrame.Content is Home))
                    {
                        ContentFrame.Navigate(typeof(Home), schedule);
                    }
                    break;
                case "Schedule":
                    if (!(ContentFrame.Content is Schedule))
                    {
                        ContentFrame.Navigate(typeof(Schedule), schedule); 
                    }
                    break;
                case "Score":
                    if (!(ContentFrame.Content is Score))
                    {
                        ContentFrame.Navigate(typeof(Score), new ScorePageNavigationParameter(token, score)); 
                    }
                    break;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is MainPageNavigationParameter p)
            {
                userId = p.UserId;
                token = p.Token;
                info = p.StudentInfo;
                score = p.Score;
                schedule = p.Schedule;

                HeaderControl.DataContext = new HeaderInfo(p.StudentInfo, p.UserId, p.Score.GPA);

                ContentFrame.Navigate(typeof(Home), schedule); 
            }

            // Reselect campus and reschedule tasks.
            if(CommonResources.LaunchState)
            {
                CommonResources.LaunchState = false;
                if(CommonResources.LoadSetting("campus", out _) == false)
                {
                    CampusSelect campusSelect = new CampusSelect();
                    await campusSelect.ShowAsync();
                    CommonResources.LoadSetting("campus", out _);
                }

                var tasks = BackgroundTaskRegistration.AllTasks;
                foreach (var task in tasks)
                {
                    task.Value.Unregister(true);
                }
                BackgroundExecutionManager.RemoveAccess();

                var backgroundStatus = await BackgroundExecutionManager.RequestAccessAsync();
                if (backgroundStatus == BackgroundAccessStatus.DeniedBySystemPolicy ||
                    backgroundStatus == BackgroundAccessStatus.DeniedByUser ||
                    backgroundStatus == BackgroundAccessStatus.Unspecified)
                {
                    return;
                }

                var builder = new BackgroundTaskBuilder();
                builder.Name = "Hourly Tile Update Task";
                builder.SetTrigger(new TimeTrigger(30, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                builder.IsNetworkRequested = true;
                builder.Register();
                builder.Name = "Login Tile Update Task";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
                builder.Register();
            } 

            await ScheduleNotificationUpdateTasks.UpdateTile(schedule);

        }

        private async void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(Login), "logout");
            CommonResources.LaunchState = true;
            await ScheduleNotificationUpdateTasks.UpdateTile(null);
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            NavPane.SelectedItem = null;

            if (!(ContentFrame.Content is Settings))
            {
                ContentFrame.Navigate(typeof(Settings));
            }
        }
    }

    class HeaderInfo : INotifyPropertyChanged
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Major { get; set; }
        public string Class { get; set; }
        public string Year { get; set; }
        public double GPA { get; set; }

        public HeaderInfo() { }
        public HeaderInfo(Model.StudentInfo info, string userId, double gpa)
        {
            UserId = userId;
            Name = info.Name;
            Major = info.Major;
            Class = info.Class;
            Year = info.Year;
            GPA = gpa;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    class MainPageNavigationParameter
    {
        public MainPageNavigationParameter(string userId, string token, Model.StudentInfo studentInfo, Model.Score score, Model.Schedule schedule)
        {
            UserId = userId;
            Token = token;
            StudentInfo = studentInfo;
            Score = score;
            Schedule = schedule;
        }

        public string UserId { get; set; }
        public string Token { get; set; }
        public Model.StudentInfo StudentInfo { get; set; }
        public Model.Score Score { get; set; }
        public Model.Schedule Schedule { get; set; }
    }
}
