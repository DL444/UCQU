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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UCqu
{
    public sealed partial class ScheduleItem : UserControl
    {
        public ScheduleItem()
        {
            this.InitializeComponent();
        }

        ScheduleEntry entry;
        public ScheduleEntry Entry
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
            bool uiFallback = CommonResources.ApiContract < 5;
            int hash = entry.Name.GetHashCode() % 4;
            if(hash < 0) { hash = -hash; }
            if(uiFallback)
            {
                string backgroundKey = "ScheduleTile" + hash.ToString() + "Fallback";
                BackgroundGrid.Background = (SolidColorBrush)this.Resources[backgroundKey];
            }
            else
            {
                BackgroundGrid.Background = TileAcrylicBrushResources.TileAcrylicBrushes[hash];
            }
            CourseNameBox.Text = entry.Name;
            RoomBox.Text = entry.Room;
        }
    }

    public static class TileAcrylicBrushResources
    {
        public static ImmutableArray<AcrylicBrush> TileAcrylicBrushes { get; } =
            ImmutableArray.Create
            (
                new AcrylicBrush()
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight1"],
                    TintOpacity = 0.8,
                    FallbackColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight1"],
                },
                new AcrylicBrush()
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColor"],
                    TintOpacity = 0.8,
                    FallbackColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColor"],
                },
                new AcrylicBrush()
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"],
                    TintOpacity = 0.8,
                    FallbackColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"],
                },
                new AcrylicBrush()
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark2"],
                    TintOpacity = 0.8,
                    FallbackColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark2"],
                }
            );
    }
}
