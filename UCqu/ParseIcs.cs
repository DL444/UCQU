using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CquScoreLib;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace UCqu
{
    public static class IcsHelper
    {
        public static string GenerateIcs(CquScoreLib.Schedule schedule)
        {
            int dayCount = schedule.Count * 7;
            DateTime firstDay = CommonResources.StartDate;
            Calendar calendar = new Calendar();
            for(int i = 0; i < dayCount; i++)
            {
                DateTime date = firstDay.AddDays(i);
                List<CquScoreLib.ScheduleEntry> dayEntries = schedule.GetDaySchedule(i);
                foreach(CquScoreLib.ScheduleEntry e in dayEntries)
                {
                    (var start, var end) = SessionTimeConverter.ConvertShort(e.SessionSpan);
                    CalendarEvent calEvent = new CalendarEvent()
                    {
                        DtStart = new CalDateTime(new DateTime(date.Year, date.Month, date.Day, start.Hour, start.Minute, 0)),
                        DtEnd = new CalDateTime(new DateTime(date.Year, date.Month, date.Day, end.Hour, end.Minute, 0)),
                        Location = e.Room,
                        Summary = e.Name,
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
