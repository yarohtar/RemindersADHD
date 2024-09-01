using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models.Scheduling
{
    public interface ISchedule
    {
        public List<ItemTime> TimesOnDate (DateTime date);
        public bool Overdue { get; }
    }

    public class ItemTime : INotifyPropertyChanged
    {
        private DateTime _date;
        public DateTime Date { get => _date; set { _date = value.Date; OnPropertyChanged(); } }

        private TimeSpan _time = TimeSpan.Zero;
        public TimeSpan Time { get => _time; set { _time = value; OnPropertyChanged(); HasTimeOfDay = true; } }


        private bool _hasTimeOfDay = false;
        public bool HasTimeOfDay { get => _hasTimeOfDay; set { _hasTimeOfDay = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string name=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
