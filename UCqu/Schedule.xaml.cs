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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter as Watcher != null)
            {
                watcher = e.Parameter as Watcher;
                CquScoreLib.Schedule schedule = watcher.Schedule;



                //await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                //    () =>
                //    {
                //        for (int i = 1; i <= schedule.Count; i++)
                //        {
                //            ScheduleFrame frame = new ScheduleFrame();
                //            frame.Schedule = schedule;
                //            frame.Week = i;
                //            WeekFlip.Items.Add(frame);
                //        }
                //    }
                //    );


                for (int i = 1; i <= schedule.Count; i++)
                {
                    ScheduleFrame frame = new ScheduleFrame();
                    frame.Schedule = schedule;
                    frame.Week = i;
                    WeekFlip.Items.Add(frame);
                }
            }

        }

        private async void ExportCalendarBtn_Click(object sender, RoutedEventArgs e)
        {
            string icsString = IcsHelper.GenerateIcs(watcher.Schedule);
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
