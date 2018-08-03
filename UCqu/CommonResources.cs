using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace UCqu
{
    public class CommonResources
    {
        public static bool LaunchState { get; set; } = true;

        public const string CurrentTerm = "20180";
        public static DateTime StartDate => new DateTime(2018, 9, 3);
        public static DateTime TestDate => new DateTime(2018, 9, 3);

        public static void SaveSetting(string key, string value)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }
        public static bool LoadSetting(string key, out string value)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string val = localSettings.Values[key] as string;
            if (val == null) { value = ""; return false; }
            else { value = val; return true; }
        }

        public static ImmutableArray<ScheduleTime> StartTimeABC { get; } =
            new ImmutableArray<ScheduleTime>
        {
            new ScheduleTime(08, 00),
            new ScheduleTime(08, 55),
            new ScheduleTime(10, 10),
            new ScheduleTime(11, 05),
            new ScheduleTime(14, 30),
            new ScheduleTime(15, 25),
            new ScheduleTime(16, 40),
            new ScheduleTime(17, 35),
            new ScheduleTime(19, 30),
            new ScheduleTime(20, 25),
            new ScheduleTime(21, 20),
        };

        public static ImmutableArray<ScheduleTime> EndTimeABC { get; } =
            new ImmutableArray<ScheduleTime>
        {
            new ScheduleTime(08, 45),
            new ScheduleTime(09, 40),
            new ScheduleTime(10, 55),
            new ScheduleTime(11, 50),
            new ScheduleTime(15, 15),
            new ScheduleTime(16, 10),
            new ScheduleTime(17, 25),
            new ScheduleTime(18, 20),
            new ScheduleTime(20, 15),
            new ScheduleTime(21, 10),
            new ScheduleTime(22, 05),
        };

        public static ImmutableArray<ScheduleTime> StartTimeD { get; } =
            new ImmutableArray<ScheduleTime>
        {
            new ScheduleTime(08, 30),
            new ScheduleTime(09, 25),
            new ScheduleTime(10, 30),
            new ScheduleTime(11, 25),
            new ScheduleTime(14, 00),
            new ScheduleTime(14, 55),
            new ScheduleTime(16, 00),
            new ScheduleTime(16, 55),
            new ScheduleTime(19, 00),
            new ScheduleTime(19, 55),
            new ScheduleTime(20, 50),
        };

        public static ImmutableArray<ScheduleTime> EndTimeD { get; } =
            new ImmutableArray<ScheduleTime>
        {
            new ScheduleTime(09, 15),
            new ScheduleTime(10, 10),
            new ScheduleTime(11, 15),
            new ScheduleTime(12, 10),
            new ScheduleTime(14, 45),
            new ScheduleTime(15, 40),
            new ScheduleTime(16, 45),
            new ScheduleTime(17, 40),
            new ScheduleTime(19, 45),
            new ScheduleTime(20, 40),
            new ScheduleTime(21, 35),
        };

        public struct ScheduleTime
        {
            private TimeSpan ts;

            public int Hour => ts.Hours;
            public int Minute => ts.Minutes;

            public ScheduleTime(int hour, int minute)
            {
                ts = new TimeSpan(hour, minute, 0);
            }

            public DateTime GetDateTime()
            {
                return DateTime.Now + ts;
            }

            public override string ToString()
            {
                return $"{Hour}:{Minute:00}";
                // TODO: 12-hour format?
            }
        }
    }
    public class SessionTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string valueStr = value as string;
            if (valueStr == null) { return ""; }
            string[] segements = valueStr.Split('-');
            CommonResources.LoadSetting("campus", out string campus);
            bool isCampusD = campus == "D" ? true : false;

            if (segements.Length == 2)
            {
                int start = int.Parse(segements[0]);
                int end = int.Parse(segements[1]);
                if(start > 11) { start = 11; }
                if(end > 11) { end = 11; }
                if(isCampusD)
                {
                    return $"{CommonResources.StartTimeD[start - 1]}-{CommonResources.EndTimeD[end - 1]}";
                }
                else
                {
                    return $"{CommonResources.StartTimeABC[start - 1]}-{CommonResources.EndTimeABC[end - 1]}";
                }
            }
            else if (segements.Length == 1)
            {
                int session = int.Parse(segements[0]);
                if (session > 11) { session = 11; }
                if (isCampusD)
                {
                    return $"{CommonResources.StartTimeD[session - 1]}-{CommonResources.EndTimeD[session - 1]}";
                }
                else
                {
                    return $"{CommonResources.StartTimeABC[session - 1]}-{CommonResources.EndTimeABC[session - 1]}";
                }
            }
            else { return ""; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static (DateTime start, DateTime end) Convert(string value)
        {
            (var start, var end) = ConvertShort(value);
            return (start.GetDateTime(), end.GetDateTime());
        }
        public static (CommonResources.ScheduleTime start, CommonResources.ScheduleTime end) ConvertShort(string value)
        {
            string[] segements = value.Split('-');
            CommonResources.LoadSetting("campus", out string campus);
            bool isCampusD = campus == "D" ? true : false;

            if (segements.Length == 2)
            {
                int start = int.Parse(segements[0]);
                int end = int.Parse(segements[1]);
                if (start > 11) { start = 11; }
                if (end > 11) { end = 11; }
                if (isCampusD)
                {
                    return (CommonResources.StartTimeD[start - 1], CommonResources.EndTimeD[end - 1]);
                }
                else
                {
                    return (CommonResources.StartTimeABC[start - 1], CommonResources.EndTimeABC[end - 1]);
                }
            }
            else if (segements.Length == 1)
            {
                int session = int.Parse(segements[0]);
                if (session > 11) { session = 11; }
                if (isCampusD)
                {
                    return (CommonResources.StartTimeD[session - 1], CommonResources.EndTimeD[session - 1]);
                }
                else
                {
                    return (CommonResources.StartTimeABC[session - 1], CommonResources.EndTimeABC[session - 1]);
                }
            }
            else
            {
                throw new ArgumentException($"Argument Format Not Valid. Argument: {value}");
            }
        }
    }


}
