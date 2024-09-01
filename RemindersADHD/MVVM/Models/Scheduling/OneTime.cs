using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models.Scheduling
{
    public class OneTime : ISchedule
    {
        public OneTime() { }

        public List<ItemTime> TimesOnDate(DateTime date)
        {
            if (Date != date.Date) return [];
            if (HasTime)
                return [new ItemTime { Date = date, Time =Time }];
            return [new ItemTime { Date = date }];
        }
        public bool Overdue => DateTime.Now > Date + Time;
        private DateTime _date;
        public DateTime Date { get => _date; set { _date = value.Date; } }
        public bool HasTime { get; set; } = false;
        public TimeSpan Time { get; set; } = TimeSpan.Zero;
    }
}
