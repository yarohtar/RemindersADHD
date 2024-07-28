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
    public partial class ShoppingItemEditViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        ShoppingItem item;

        public string Name { get => item.Name; set => item.Name = value; }
        public DateTime? LastDateBought { get => item.TimesBought==0 ? null : item.LastBought; set => item.LastBought = (DateTime)value!; }
        public string BuyAgainDaysString { get => item.BuyAgainDaysString; set => item.BuyAgainDaysString = value; }
        public bool HasBeenBought { get => item.TimesBought > 0; }
        public bool HasNotBeenBought { get => !HasBeenBought; }

        public ICommand SaveCommand => new Command(async () => await Save());
        bool isbusy;

        public ShoppingItemEditViewModel()
        {
            item = new ShoppingItem();
        }

        public void Initialize()
        {
            isbusy = false;
            return;
        }
        private async Task Save()
        {
            if (isbusy) return;
            isbusy = true;
            await ShoppingDataService.UpdateItem(item);
            await Shell.Current.GoToAsync("..", true); //doesn't animate????? why??????
        }
        
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int itemId = int.Parse((query["itemId"] as string)!);
            item = await ShoppingDataService.GetItem(itemId);
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(LastDateBought));
            OnPropertyChanged(nameof(BuyAgainDaysString));
            OnPropertyChanged(nameof(HasBeenBought));
            OnPropertyChanged(nameof(HasNotBeenBought));
        }
    }
}
