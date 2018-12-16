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
using Model = DL444.UcquLibrary.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        Model.Schedule schedule;
        List<Model.ScheduleEntry> dayEntries = null;
        List<HuxiImgEntry> huxiImgEntries = null;

        public Home()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Model.Schedule schedule)
            {
                this.schedule = schedule;

                dayEntries = schedule.GetDaySchedule((DateTime.Today - CommonResources.StartDate).Days).ToList();
                if (dayEntries.Count != 0)
                {
                    dayEntries.Sort();
                    TodayScheduleList.ItemsSource = dayEntries;
                    TodayScheduleList.Visibility = Visibility.Visible;
                }
            }

            if (huxiImgEntries == null)
            {
                huxiImgEntries = await HuxiImg.GetEntries();
            }

            if(huxiImgEntries.Count == 0)
            {
                HuxiImgGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                HuxiImgGrid.ItemsSource = huxiImgEntries;
            }

        }
    }
}
