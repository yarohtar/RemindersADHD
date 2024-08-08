using RemindersADHD.MVVM.Models;
using RemindersADHD.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemindersADHD.MVVM.ViewModels
{
    public class HabitEditViewModel : INotifyPropertyChanged, IQueryAttributable
    {

        private Habit habit;

        public string Name { get=>habit.Name; set=>habit.Name=value; }
        public string TimesPer { get => habit.TimesPer.ToString(); 
            set
            {
                string filter = new string(value.Where(char.IsDigit).ToArray());
                filter = filter.TrimStart('0');
                if (filter == "")
                    filter = "0";
                habit.TimesPer = int.Parse(filter);
                OnPropertyChanged();
            }
        }

        public string Period { get => habit.Period.ToString();
            set
            {
                string filter = new string(value.Where(char.IsDigit).ToArray());
                filter = filter.TrimStart('0');
                if (filter == "")
                    filter = "0";
                habit.Period = int.Parse(filter);
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand => new Command(async () => await Save());
        public ICommand DeleteCommand => new Command(async () => await Delete());

        public HabitEditViewModel()
        {
            habit= new Habit();
        }
        private bool isbusy = false;
        private async Task Save()
        {
            if (isbusy) return;
            isbusy = true;
            if(habit.Name=="")
            {
                return;
            }
            await HabitsDataService.UpdateItem(habit);
            await Shell.Current.GoToAsync("..", true); //doesn't animate????? why??????
        }

        private async Task Delete()
        {
            if (isbusy) return;
            isbusy = true;
            if(habit.Name=="")
            {
                await HabitsDataService.RemoveItem(habit.Id);
                await Shell.Current.GoToAsync("..", true);
                return;
            }

        }

        public void Initialize()
        {
            isbusy = false;
            return;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query is null)
                throw new Exception();
            if(!query.TryGetValue("Id", out object? s))
                throw new Exception();

            int habitId = int.Parse((s as string)!);
            habit = await HabitsDataService.GetItem(habitId);
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(TimesPer));
            OnPropertyChanged(nameof(Period));
        }
    }
}
