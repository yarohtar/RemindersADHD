using RemindersADHD.MVVM.Models;
using RemindersADHD.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RemindersADHD.MVVM.ViewModels
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private IList<ItemKind> _itemKinds = [];

        public ObservableCollection<ICardBindable> Items { get; private set; } = [];
        public ObservableCollection<ICardBindable> CompletedItems { get; private set; } = [];

        private string _newItemName = "";
        public string NewItemName
        {
            get => _newItemName;
            set
            {
                _newItemName = value.Length > 25 ? value.Substring(0, 25) : value;
                OnPropertyChanged();
            }
        }

        public ICommand AddNewItemCommand => new Command(async () => await AddNewItem());
        public ICommand CheckChangedCommand => new Command(async (object item) => await CheckChanged(item as ICardBindable));

        private async Task AddNewItem()
        {
            ItemKind kind = new ItemKind(new Tracker()) { Title = NewItemName };
            await ItemDataService.AddItem(kind);
            _itemKinds.Add(kind);
            Items.Add(kind.GetItemNoDate());
            NewItemName = "";
        }

        private async Task CheckChanged(ICardBindable? item)
        {
            if (item is null) { return; }
            if (!item.Kind.IsDoable) return;
            if (item.Done && Items.Remove(item))
            {
                CompletedItems.Add(item);
                await ItemDataService.AddNoDateDone(item.Kind.Tracker, item.Kind);
            }
            else if (!item.Done && CompletedItems.Remove(item))
            {
                Items.Add(item);
                await ItemDataService.RemoveNoDateDone(item.Kind.Tracker, item.Kind);
            }
        }

        public async Task Initialize()
        {
            _itemKinds = await ItemDataService.GetItems();
            Items.Clear();
            CompletedItems.Clear();
            foreach (var item in _itemKinds)
            {
                var i = item.GetItemNoDate();
                if (!i.Done)
                    Items.Add(i);
                else
                    CompletedItems.Add(i);
            }
            await ItemDataService.FilterTrackers();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
