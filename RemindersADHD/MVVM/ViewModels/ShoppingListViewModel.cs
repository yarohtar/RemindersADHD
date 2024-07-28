using MvvmHelpers;
using RemindersADHD.MVVM.Models;
using RemindersADHD.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemindersADHD.MVVM.ViewModels
{
    public class ShoppingListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        public ObservableRangeCollection<ShoppingItem> ShoppingItems { get; set; }
        public ObservableRangeCollection<ShoppingItem> OldItems { get; set; }

        public ICommand AddNewItemCommand => new Command(async () => await AddNewItem());
        public ICommand AddOldItemCommand => new Command(async (item) => await AddItem((ShoppingItem)item));
        public ICommand RemoveShoppingItemCommand => new Command(async (item) => await RemoveShoppingItem((ShoppingItem)(item)));
        public ICommand DeleteOldItemCommand => new Command(async (item) => await RemoveOldItem((ShoppingItem)(item)));

        public ICommand BuyItemCommand => new Command(async (item) => await BuyShoppingItem((ShoppingItem)item));

        public ICommand SwitchItemNote => new Command(async (item) =>
        {
            ((ShoppingItem)item).HasNoteSwitch();
            await ShoppingDataService.UpdateItem((ShoppingItem)item);
        });

        public ICommand NavigateToEditItemCommand => new Command(async (item) =>
        await NavigateToEditItem((ShoppingItem)item));

        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(); }
        }

        private string newItemName = "";
        public string NewItemName
        {
            get
            {
                return newItemName;
            }
            set
            {
                newItemName = value;
                OnPropertyChanged();
            }
        }

        public ShoppingListViewModel() { 
            ShoppingItems = new ObservableRangeCollection<ShoppingItem>();
            OldItems = new ObservableRangeCollection<ShoppingItem>();
        }

        public async Task Initialize()
        {
            var items1 = (await ShoppingDataService.GetItemsWhere(c => c.IsOnList)).ToList();
            var items2 = (await ShoppingDataService.GetItemsWhere(c => !c.IsOnList)).ToList(); //baddddddddd ah
            items1.Sort(ShoppingItem.Compare);
            items2.Sort(ShoppingItem.Compare);
            ShoppingItems.Clear();
            OldItems.Clear();
            ShoppingItems.AddRange(items1);
            OldItems.AddRange(items2);
            IsBusy = false;
        }

        private async Task NavigateToEditItem(ShoppingItem item)
        {
            if (IsBusy) return;
            IsBusy = true;
            await Shell.Current.GoToAsync($"shoppingitemedit?itemId={item.Id}");
        }


        private async Task AddNewItem()
        {
            if (NewItemName == "")
                return;
            if (ShoppingItems.FirstOrDefault(x => x.Name == NewItemName) != null)
            {
                NewItemName = "";
                return;
            }
            var s = OldItems.FirstOrDefault(x=>x.Name == NewItemName);
            if (s != null)
            {
                NewItemName = "";
                await AddItem(s);
                return;
            }

            ShoppingItem item = new ShoppingItem(NewItemName);
            await ShoppingDataService.AddItem(item);
            await AddItem(item);
            NewItemName = "";
        }

        private async Task AddItem(ShoppingItem item)
        {
            await AddBinary(ShoppingItems, item, 0, ShoppingItems.Count - 1);
            OldItems.Remove(item);
            item.IsOnList = true;
            await ShoppingDataService.UpdateItem(item);
        }

        private async Task RemoveOldItem(ShoppingItem s)
        {
            OldItems.Remove(s);
            await ShoppingDataService.RemoveItem(s.Id);
        }

        private async Task BuyShoppingItem(ShoppingItem s)
        {
            s.Buy();
            ShoppingItems.Remove(s);
            s.IsOnList = false;
            await AddBinary(OldItems, s, 0, OldItems.Count - 1);
            await ShoppingDataService.UpdateItem(s);
        }
        private async Task RemoveShoppingItem(ShoppingItem s)
        {
            ShoppingItems.Remove(s);
            s.IsOnList = false;
            await AddBinary(OldItems, s, 0, OldItems.Count-1);
            await ShoppingDataService.UpdateItem(s);
        }

        private async Task AddBinary(IList<ShoppingItem> list, ShoppingItem item, int l, int r) //probably bad lmao
        {
            if (l >= r) { list.Insert(l, item); return; }
            int m=(l+r)/2;
            if (ShoppingItem.Compare(item, list[m])>=0) { await AddBinary(list, item, l, m); }
            else { await AddBinary(list, item, m+1, r); }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
