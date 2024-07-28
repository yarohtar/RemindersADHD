using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models
{
    public class HabitOnDay(Habit habit, DateTime date) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public Habit Habit { get; } = habit;
        private DateTime date = date;
        public string Name { get => Habit.Name; }
        public DateTime Date
        {
            get => date;
            set
            {
                if (date == value.Date) return;
                date = value.Date;
                done = Habit.DoneOnDate(date);
                OnPropertyChanged(nameof(Done));
            }
        }

        private bool done = habit.DoneOnDate(date);
        public bool Done
        {
            get => done;
            set
            {
                if (done == value) return;
                done = value;
                Habit.DoneOnDateChanged(value, Date);
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged is null)
                return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
