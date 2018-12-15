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
using Windows.Foundation.Metadata;
using System.Collections.Immutable;
using Model = DL444.UcquLibrary.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UCqu
{
    public sealed partial class ScheduleItem : UserControl
    {
        public ScheduleItem()
        {
            this.InitializeComponent();
        }

        Model.ScheduleEntry entry;
        public Model.ScheduleEntry Entry
        {
            get => entry;
            set
            {
                entry = value;
                Draw();
            }
        }
        void Draw()
        {
            int hash = entry.Name.GetHashCode() % 4;
            if (hash < 0) { hash = -hash; }
            string backgroundKey = "ScheduleTile" + hash.ToString();
            BackgroundGrid.Background = (AcrylicBrush)this.Resources[backgroundKey];
            CourseNameBox.Text = entry.Name;
            RoomBox.Text = entry.Room;
        }
    }
}
