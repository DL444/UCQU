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
    public sealed partial class Home : Page
    {
        Watcher watcher = null;
        List<ScheduleEntry> dayEntries = null;
        List<HuxiImgEntry> huxiImgEntries = null;
        Random randomizer = new Random(DateTime.Now.Millisecond);

        DateTime startDate = new DateTime(2018, 9, 3);
        DateTime testDate = new DateTime(2018, 9, 3);

        public Home()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter as Watcher != null)
            {
                watcher = e.Parameter as Watcher;
                ScoreSet set = watcher.GetSet((watcher.Workload as SingleWorkload).Workload + "_0");
                HeaderControl.Content = set;

                dayEntries = watcher.Schedule.GetDaySchedule((DateTime.Today - startDate).Days);
                if (dayEntries.Count != 0)
                {
                    dayEntries.Sort();
                    TodayScheduleList.Visibility = Visibility.Visible;
                    TodayScheduleList.ItemsSource = dayEntries;
                }
            }

            if (huxiImgEntries == null)
            {
                huxiImgEntries = await HuxiImg.GetEntries();
            }

            HuxiImgGrid.ItemsSource = huxiImgEntries;
        }
    }
}
