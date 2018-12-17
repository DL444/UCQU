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
    public sealed partial class Schedule : Page
    {
        Model.Schedule schedule;

        public Schedule()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.schedule = RuntimeData.Schedule;
            for (int i = 0; i < schedule.Count; i++)
            {
                ScheduleFrame frame = new ScheduleFrame();
                frame.Week = schedule.Weeks[i].WeekNumber;
                frame.WeekSchedule = schedule.Weeks[i];
                WeekFlip.Items.Add(frame);
            }
            int elapsedWeeks = (DateTime.Today - RuntimeData.StartDate).Days / 7;
            if (elapsedWeeks > schedule.Count - 1)
            {
                WeekFlip.SelectedIndex = schedule.Count - 1;
            }
            else
            {
                WeekFlip.SelectedIndex = elapsedWeeks;
            }
        }

        private async void ExportCalendarBtn_Click(object sender, RoutedEventArgs e)
        {
            string icsString = IcsHelper.GenerateIcs(schedule);
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("ICS 文件", new List<string>() { ".ics" });
            savePicker.SuggestedFileName = "课表";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if(file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await Windows.Storage.FileIO.WriteTextAsync(file, icsString);
                await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            }
        }
    }
}
