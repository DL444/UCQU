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
    public sealed partial class Schedule : Page
    {
        Watcher watcher = null;

        public Schedule()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter as Watcher != null)
            {
                watcher = e.Parameter as Watcher;
                CquScoreLib.Schedule schedule = await watcher.PerformSchedule("20180");

                for(int i = 1; i <= schedule.Count; i++)
                {
                    ScheduleFrame frame = new ScheduleFrame();
                    frame.Schedule = schedule;
                    frame.Week = i;
                    WeekFlip.Items.Add(frame);
                }
            }

        }

        private void WeekFlip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //frame.Week = WeekFlip.SelectedIndex + 1;
        }
    }
}
