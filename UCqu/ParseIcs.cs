using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Model = DL444.UcquLibrary.Models;

namespace UCqu
{
    public static class IcsHelper
    {
        public static string GenerateIcs(Model.Schedule schedule)
        {
            int dayCount = schedule.Count * 7;
            DateTime firstDay = RuntimeData.StartDate;
            Calendar calendar = new Calendar();
            for(int i = 0; i < dayCount; i++)
            {
                DateTime date = firstDay.AddDays(i);
                foreach(Model.ScheduleEntry e in schedule.GetDaySchedule(i))
                {
                    (var start, var end) = SessionTimeConverter.ConvertShort(e.SessionSpan);
                    CalendarEvent calEvent = new CalendarEvent()
                    {
                        DtStart = new CalDateTime(new DateTime(date.Year, date.Month, date.Day, start.Hour, start.Minute, 0)),
                        DtEnd = new CalDateTime(new DateTime(date.Year, date.Month, date.Day, end.Hour, end.Minute, 0)),
                        Location = e.Room,
                        Summary = e.Name,
                        Description = $"教师: {e.Lecturer}",
                        Alarms = 
                        {
                            new Alarm()
                            {
                                Trigger = new Trigger(new TimeSpan(0, -15, 0))
                            }
                        }
                    };
                    calendar.Events.Add(calEvent);
                }
            }
            var serializer = new Ical.Net.Serialization.CalendarSerializer();
            return serializer.SerializeToString(calendar);
        }
    }
}
