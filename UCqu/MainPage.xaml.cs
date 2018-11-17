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
using CquScoreLib;
using Windows.ApplicationModel.Background;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Watcher watcher = null;

        public string StuName { get; private set; }
        public string Year { get; private set; }
        public string Major { get; private set; }
        public string Class { get; private set; }
        public string Id { get; private set; }
        // TODO: This is not notifying! Consider move to new class.

        //bool firstNavigate = true;
        //bool isCampusD = false;
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
            //HomeBtn.IsChecked = false;
            //ScheduleBtn.IsChecked = false;
            //ScoreBtn.IsChecked = false;

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
            if (watcher == null) { return; }
            HeaderControl.Visibility = selectionTag == "Home" ? Visibility.Visible : Visibility.Collapsed;
            switch (selectionTag)
            {
                case "Home":
                    if (!(ContentFrame.Content is Home))
                    {
                        ContentFrame.Navigate(typeof(Home), watcher);
                    }
                    break;
                case "Schedule":
                    if (!(ContentFrame.Content is Schedule))
                    {
                        ContentFrame.Navigate(typeof(Schedule), watcher);
                    }
                    break;
                case "Score":
                    if (!(ContentFrame.Content is Score))
                    {
                        ContentFrame.Navigate(typeof(Score), watcher);
                    }
                    break;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Watcher w)
            {
                watcher = w;
                Id = watcher.UserName;
                //HomeBtn.IsChecked = true;
                HeaderControl.Content = watcher;
                ContentFrame.Navigate(typeof(Home), watcher);
            }

            if(CommonResources.LaunchState)
            {
                CommonResources.LaunchState = false;
                //string campus = "";
                if(CommonResources.LoadSetting("campus", out _) == false)
                {
                    CampusSelect campusSelect = new CampusSelect();
                    await campusSelect.ShowAsync();
                    CommonResources.LoadSetting("campus", out _);
                    //if (campus == "D") { isCampusD = true; }
                }
                //else
                //{
                //    if(campus == "D")
                //    {
                //        isCampusD = true;
                //    }
                //}

                var tasks = BackgroundTaskRegistration.AllTasks;
                foreach (var task in tasks)
                {
                    task.Value.Unregister(false);
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

            await ScheduleNotificationUpdateTasks.UpdateTile();

        }

        private async void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(Login), "logout");
            await ScheduleNotificationUpdateTasks.UpdateTile();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            NavPane.SelectedItem = null;
            //HomeBtn.IsChecked = false;
            //ScheduleBtn.IsChecked = false;
            //ScoreBtn.IsChecked = false;

            if (!(ContentFrame.Content is Settings))
            {
                ContentFrame.Navigate(typeof(Settings));
            }
        }

        //private void NavBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    string selectionTag = (sender as RadioButton).Tag.ToString();
        //    if (watcher == null) { return; }
        //    switch (selectionTag)
        //    {
        //        case "Home":
        //            if (!(ContentFrame.Content is Home))
        //            {
        //                ContentFrame.Navigate(typeof(Home), watcher);
        //            }
        //            break;
        //        case "Schedule":
        //            if (!(ContentFrame.Content is Schedule))
        //            {
        //                ContentFrame.Navigate(typeof(Schedule), watcher);
        //            }
        //            break;
        //        case "Score":
        //            if (!(ContentFrame.Content is Score))
        //            {
        //                ContentFrame.Navigate(typeof(Score), watcher);
        //            }
        //            break;
        //    }

        //}
    }
}
