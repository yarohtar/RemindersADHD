using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemindersADHD.MVVM.Models
{
    public class Habit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int TimesPer { get; set; }
        public int Period { get; set; }


        public HashSet<DateTime> datesCompleted = [];
        [Ignore]
        public bool Overdue { get => TimesDoneInLastPeriod < TimesPer; }
        [Ignore]
        public int TimesDoneInLastPeriod { get => datesCompleted.Count((d) => d.Date.AddDays(Period) > DateTime.Now.Date); }

        public void DoneOnDateChanged(bool value, DateTime date)
        {
            if (value) AddDate(date);
            else RemoveDate(date);
        }

        public bool DoneOnDate(DateTime date)
        {
            return datesCompleted.Any(d => d.Date == date.Date);
        }
        public HabitOnDay GetHabitOnDay(DateTime date)
        {
            return new HabitOnDay(this, date);
        }

        public void AddDate(DateTime date)
        {
            date = date.Date;
            datesCompleted.Add(date);
        }
        public void RemoveDate(DateTime date)
        {
            date = date.Date;
            datesCompleted.Remove(date);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
