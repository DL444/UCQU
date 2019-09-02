using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Data;
using Model = DL444.UcquLibrary.Models;

namespace UCqu
{
    static class RuntimeData
    {
        public static bool LaunchState { get; set; } = true;
        public static DateTime StartDate { get; set; }
#if DEBUG
        public static DateTime TestDate => new DateTime(2018, 9, 3);
#endif

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

        public static ImmutableArray<Model.ScheduleTime> StartTimeABC { get; set; }
        public static ImmutableArray<Model.ScheduleTime> EndTimeABC { get; set; }
        public static ImmutableArray<Model.ScheduleTime> StartTimeD { get; set; }
        public static ImmutableArray<Model.ScheduleTime> EndTimeD { get; set; }

        public static string UserId { get; set; }
        public static string Token { get; set; }
        public static Model.StudentInfo  StudentInfo { get; set; }
        public static Model.Score Score { get; set; }
        public static Model.Score SecondMajorScore { get; set; }
        public static Model.Schedule Schedule { get; set; }
    }

    public class SessionTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string valueStr)) { return ""; }
            string[] segements = valueStr.Split('-');
            RuntimeData.LoadSetting("campus", out string campus);
            bool isCampusD = campus == "D" ? true : false;

            if (segements.Length == 2)
            {
                int start = int.Parse(segements[0]);
                int end = int.Parse(segements[1]);
                if(start > 11) { start = 11; }
                if(end > 11) { end = 11; }
                if(isCampusD)
                {
                    return $"{RuntimeData.StartTimeD[start - 1]}-{RuntimeData.EndTimeD[end - 1]}";
                }
                else
                {
                    return $"{RuntimeData.StartTimeABC[start - 1]}-{RuntimeData.EndTimeABC[end - 1]}";
                }
            }
            else if (segements.Length == 1)
            {
                int session = int.Parse(segements[0]);
                if (session > 11) { session = 11; }
                if (isCampusD)
                {
                    return $"{RuntimeData.StartTimeD[session - 1]}-{RuntimeData.EndTimeD[session - 1]}";
                }
                else
                {
                    return $"{RuntimeData.StartTimeABC[session - 1]}-{RuntimeData.EndTimeABC[session - 1]}";
                }
            }
            else { return ""; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }

        //public static (DateTime start, DateTime end) Convert(string value)
        //{
        //    (var start, var end) = ConvertShort(value);
        //    return (start.GetDateTime(), end.GetDateTime());
        //}
        public static (Model.ScheduleTime start, Model.ScheduleTime end) ConvertShort(string value)
        {
            RuntimeData.LoadSetting("campus", out string campus);
            bool isCampusD = campus == "D" ? true : false;
            return ConvertShort(value, isCampusD);
        }
        public static (Model.ScheduleTime start, Model.ScheduleTime end) ConvertShort(string value, bool isCampusD)
        {
            string[] segements = value.Split('-');
            if (segements.Length == 2)
            {
                int start = int.Parse(segements[0]);
                int end = int.Parse(segements[1]);
                if (start > 11) { start = 11; }
                if (end > 11) { end = 11; }
                if (isCampusD)
                {
                    return (RuntimeData.StartTimeD[start - 1], RuntimeData.EndTimeD[end - 1]);
                }
                else
                {
                    return (RuntimeData.StartTimeABC[start - 1], RuntimeData.EndTimeABC[end - 1]);
                }
            }
            else if (segements.Length == 1)
            {
                int session = int.Parse(segements[0]);
                if (session > 11) { session = 11; }
                if (isCampusD)
                {
                    return (RuntimeData.StartTimeD[session - 1], RuntimeData.EndTimeD[session - 1]);
                }
                else
                {
                    return (RuntimeData.StartTimeABC[session - 1], RuntimeData.EndTimeABC[session - 1]);
                }
            }
            else
            {
                throw new ArgumentException($"Argument Format Not Valid. Argument: {value}");
            }
        }
    }

    public static class CampusSelector
    {
        public static bool IsCampusD(string room)
        {
            if(room == null)
            {
                room = "";
            }
            if(room.StartsWith("D"))
            {
                return true;
            }
            else if(room.StartsWith("A") || room.StartsWith("B") || room.StartsWith("C"))
            {
                return false;
            }
            else
            {
                RuntimeData.LoadSetting("campus", out string campus);
                return campus == "D" ? true : false;
            }
        }
    }
}
