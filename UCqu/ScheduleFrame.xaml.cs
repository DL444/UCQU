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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UCqu
{
    public sealed partial class ScheduleFrame : UserControl
    {
        private int _week = 1;
        private CquScoreLib.Schedule _schedule;
        Grid mainGrid;

        public ScheduleFrame()
        {
            this.InitializeComponent();
        }

        public CquScoreLib.Schedule Schedule
        {
            get => _schedule;
            set
            {
                _schedule = value;
                Draw();
            }
        }

        public int Week
        {
            get => _week;
            set
            {
                _week = value;
                if(_schedule != null)
                {
                    Draw();
                }
            }
        }
        void Draw()
        {
            WeekText.Text = $"第{Week}周";
            SchedGrid.Children.Clear();
            List<ScheduleEntry> entries = Schedule[Week];
            bool showWeekend = false;
            bool showNightSession = false;
            foreach(ScheduleEntry entry in entries)
            {
                ScheduleItem item = new ScheduleItem();
                item.Entry = entry;
                showNightSession = showNightSession || (entry.StartSlot > 8);
                showWeekend = showWeekend || (entry.DayOfWeek > 5);
                Grid.SetRow(item, entry.StartSlot - 1);
                Grid.SetRowSpan(item, entry.EndSlot - entry.StartSlot + 1);
                Grid.SetColumn(item, entry.DayOfWeek - 1);
                SchedGrid.Children.Add(item);
            }
            ShowAdditional(showWeekend, showNightSession);
        }

        void ShowAdditional(bool showWeekend, bool showNightSession)
        {
            GridLength l0 = new GridLength(0, GridUnitType.Pixel);
            GridLength l1 = new GridLength(1, GridUnitType.Star);
            if (showWeekend)
            {
                Sat.Width = l1;
                SatH.Width = l1;
                Sun.Width = l1;
                SunH.Width = l1;
                Grid.SetColumnSpan(SchedGrid, 7);
            }
            else
            {
                Sat.Width = l0;
                Sun.Width = l0;
                SatH.Width = l0;
                SunH.Width = l0;
                Grid.SetColumnSpan(SchedGrid, 5);
            }

            if (showNightSession)
            {
                Night1.Height = l1;
                Night2.Height = l1;
                Night3.Height = l1;
                Night4.Height = l1;
                Night1H.Height = l1;
                Night2H.Height = l1;
                Night3H.Height = l1;
                Night4H.Height = l1;
                Grid.SetRowSpan(SchedGrid, 12);
            }
            else
            {
                Night1.Height = l0;
                Night2.Height = l0;
                Night3.Height = l0;
                Night4.Height = l0;
                Night1H.Height = l0;
                Night2H.Height = l0;
                Night3H.Height = l0;
                Night4H.Height = l0;
                Grid.SetRowSpan(SchedGrid, 8);
            }
        }


        public bool NextWeek()
        {
            if(Week > Schedule.Count) { return false; }
            Week++;
            return Week >= Schedule.Count;
        }
        public bool LastWeek()
        {
            if (Week > Schedule.Count) { return false; }
            Week--;
            return Week >= Schedule.Count;
        }

        //private void WeekFlip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Week = WeekFlip.SelectedIndex + 1;
        //    mainGrid = ((WeekFlip.Items[0] as FlipViewItem).Content as Grid);
        //    (WeekFlip.SelectedItem as FlipViewItem).Content = mainGrid;
        //}
    }
}
