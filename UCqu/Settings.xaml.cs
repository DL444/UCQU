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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UCqu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void CampusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CampusCombo.SelectedIndex == 0)
            {
                RuntimeData.SaveSetting("campus", "ABC");
            }
            else if(CampusCombo.SelectedIndex == 1)
            {
                RuntimeData.SaveSetting("campus", "D");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(RuntimeData.LoadSetting("campus", out string campus) == false)
            {
                CampusCombo.SelectedIndex = 0;
            }
            else
            {
                if(campus == "ABC") { CampusCombo.SelectedIndex = 0; }
                else if(campus == "D") { CampusCombo.SelectedIndex = 1; }
            }

            if(RuntimeData.LoadSetting("courseToastSwitch", out string courseSwitch) == true)
            {
                CourseToastToggle.IsOn = courseSwitch == "on" ? true : false;
            }
            if (RuntimeData.LoadSetting("dailyToastSwitch", out string dailySwitch) == true)
            {
                DailyToastToggle.IsOn = dailySwitch == "on" ? true : false;
            }
            if (RuntimeData.LoadSetting("imgToastSwitch", out string imgSwitch) == true)
            {
                HuxiImgToastToggle.IsOn = imgSwitch == "on" ? true : false;
            }

        }

        private void ToastToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch _switch = sender as ToggleSwitch;
            string str = _switch.IsOn ? "on" : "off";
            RuntimeData.SaveSetting(_switch.Tag as string, str);
        }
    }
}
