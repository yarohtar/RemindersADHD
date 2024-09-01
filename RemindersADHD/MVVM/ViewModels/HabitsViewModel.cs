using MvvmHelpers;
using RemindersADHD.MVVM.Models;
using RemindersADHD.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RemindersADHD.MVVM.ViewModels
{
    public enum TodayFlag { Today, OtherDay };
    public enum ChangingToDay { Next, Previous, Future, Same, Other };
    public class HabitsViewModel : INotifyPropertyChanged
    {

        public ObservableRangeCollection<HabitOnDay> HabitsToday { get; set; } = [];
        public ObservableRangeCollection<HabitOnDay> OtherHabits { get; set; } = [];
        public ObservableRangeCollection<HabitOnDay> HabitsOnCurrentDate { get; set; } = [];

        public IEnumerable<HabitOnDay> HelperHabits { get; set; } = [];
        public Func<ChangingToDay, Task>? DateChanging { get; set; }
        public Func<Task>? ListUpdated { get; set; }

        public IEnumerable<Habit> allHabits = [];

        private DateTime currentDate;
        public DateTime CurrentDate
        {
            get => currentDate;
            set
            {
                if (value >= DateTime.Now)
                    currentDate = DateTime.Now.Date;
                else
                    currentDate = value.Date;
                ChangeDateInList(HabitsOnCurrentDate, currentDate);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateText));
            }
        }

        public static DateTime MaxDate => DateTime.Now.Date;

        public string DateText
        {
            get
            {
                if (CurrentDate == DateTime.Now.Date)
                    return "Today";
                if (CurrentDate == DateTime.Now.AddDays(-1).Date)
                    return "Yesterday";
                return CurrentDate.ToString("dddd, dd MMM yyyy");
            }
        }

        #region Commands
        public ICommand PrevDateCommand => new Command(async () =>
        {
            Task? t = DateChanging?.Invoke(ChangingToDay.Previous);
            if (t is not null) await t;
            CurrentDate = CurrentDate.AddDays(-1);
            t = ListUpdated?.Invoke();
            if (t is not null) await t;
        });
        public ICommand NextDateCommand => new Command(async () =>
        {
            Task? t;
            if (CurrentDate.AddDays(1) > DateTime.Now.Date)
                t = DateChanging?.Invoke(ChangingToDay.Future);
            else
                t = DateChanging?.Invoke(ChangingToDay.Next);
            if (t is not null) await t;
            CurrentDate = CurrentDate.AddDays(1);
            t = ListUpdated?.Invoke();
            if (t is not null) await t;
        });
        public ICommand CheckChangedCommand => new Command(async (h) => await CheckChanged(h as HabitOnDay));
        public ICommand AddNewItemCommand => new Command(async () => await AddNewItem());
        public ICommand EditItemCommand => new Command(async (h) => await EditItem((h is HabitOnDay h1) ? h1.Habit : throw new Exception()));
        public ICommand DeleteCommand => new Command(async (h) => await DeleteItem((h is HabitOnDay h1) ? h1.Habit : throw new Exception()));
        #endregion
        public bool IsBusy { get; set; }

        public HabitsViewModel()
        {
            IsBusy = false;
            CurrentDate = DateTime.Now.Date;
        }

        public async Task Initialize()
        {
            allHabits = await HabitsDataService.GetItems();
            var l = CreateHabitsOnDay(CurrentDate);
            //foreach (var item in l)
            //    HabitsOnCurrentDate.Add(item);
            HabitsOnCurrentDate.ReplaceRange(l);
            IsBusy = false;
        }
        public void SetHelperNext()
        {
            HelperHabits = CreateHabitsOnDay(CurrentDate.AddDays(1));
            OnPropertyChanged(nameof(HelperHabits));
        }
        public void SetHelperPrev()
        {
            HelperHabits = CreateHabitsOnDay(CurrentDate.AddDays(-1));
            OnPropertyChanged(nameof(HelperHabits));
        }

        private async Task CheckChanged(HabitOnDay? habit)
        {
            if (habit is null) return;
            if (habit.Done)
                await Checked(habit);
            else
                await Unchecked(habit);
        }
        private async Task Unchecked(HabitOnDay? item)
        {
            if (item is null) return;
            await HabitsDataService.RemoveDate(item.Habit.Id, item.Date);
        }
        private async Task Checked(HabitOnDay? item)
        {
            if (item is null)
                return;
            await HabitsDataService.AddDate(item.Habit.Id, item.Date);
        }

        private async Task AddNewItem()
        {
            if (IsBusy) return;
            IsBusy = true;
            var item = new Habit { Name = "", Period = 1, TimesPer = 1 };
            await HabitsDataService.AddItem(item);
            await Shell.Current.GoToAsync($"habitedit?Id={item.Id}");
        }

        private async Task EditItem(Habit? h)
        {
            if (h is null) return;
            if (IsBusy) return;
            IsBusy = true;
            await Shell.Current.GoToAsync($"habitedit?Id={h.Id}");
        }

        private async Task DeleteItem(Habit? h)
        {
            if (h is null) return;
            await HabitsDataService.RemoveItem(h.Id);
            Refresh();
        }

        private IEnumerable<HabitOnDay> CreateHabitsOnDay(DateTime date)
        {
            if (date > DateTime.Now.Date)
                return Enumerable.Empty<HabitOnDay>();
            return allHabits.Select(x => x.GetHabitOnDay(date));
        }
        private static IEnumerable<HabitOnDay> CreateHabitsOnDay(DateTime date, IEnumerable<Habit> habits)
        {
            return habits.Select(x => new HabitOnDay(x, date));
        }
        private static void ChangeDateInList(IEnumerable<HabitOnDay> habits, DateTime newDate)
        {
            foreach (var h in habits)
            {
                h.Date = newDate;
            }
        }

        private void RefreshToday()
        {
            var l1 = allHabits.Where(h => h.Overdue);
            var l2 = allHabits.Except(l1);
            HabitsToday.ReplaceRange(CreateHabitsOnDay(DateTime.Now.Date, l1));
            OtherHabits.ReplaceRange(CreateHabitsOnDay(DateTime.Now.Date, l2));
        }
        private async void Refresh()
        {
            allHabits = await HabitsDataService.GetItems();
            HabitsOnCurrentDate.ReplaceRange(CreateHabitsOnDay(CurrentDate));
        }

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged is null)
                return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
