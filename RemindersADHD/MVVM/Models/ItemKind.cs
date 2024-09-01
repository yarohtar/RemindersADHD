using Connect;
using RemindersADHD.MVVM.Models.Scheduling;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemindersADHD.MVVM.Models
{
    public class ItemKind : INotifyPropertyChanged, IUniqueId
    {
        public ItemKind()
        {
            _subItemsReadOnly = new(_subItems);
            OnPropertyChanged(nameof(SubItems));
        }
        public ItemKind(Tracker tracker) : this()
        {
            Tracker = tracker;
        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public bool HasSchedule { get; set; } = false;

        [Ignore]
        public ISchedule Schedule { get; set; }

        [Ignore]
        public Tracker Tracker { get; set; }

        #region Cosmetics
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        public string Icon { get; set; } = "";

        #endregion

        private ObservableCollection<ItemKind> _subItems = [];
        private ReadOnlyObservableCollection<ItemKind> _subItemsReadOnly;
        private Dictionary<int, ItemKind> _subItemsDictionary = [];
        [Ignore]
        public ReadOnlyObservableCollection<ItemKind> SubItems { get => _subItemsReadOnly; }

        private bool _subItemsVisible = false;
        public bool SubItemsVisible
        {
            get => _subItemsVisible;
            set
            {
                _subItemsVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isDoable = true;
        public bool IsDoable
        {
            get => _isDoable;
            set
            {
                _isDoable = value;
                OnPropertyChanged();
            }
        }

        private static readonly IdEqualityComparer<ItemKind> _comparer = new();
        [Ignore]
        public static IEqualityComparer<IUniqueId> Comparer => _comparer;

        public bool RemoveSubItemById(int id)
        {
            if (_subItemsDictionary.ContainsKey(id))
            {
                _subItems.Remove(_subItemsDictionary[id]);
                _subItemsDictionary.Remove(id);
                return true;
            }
            return false;
        }

        public bool RemoveSubItem(ItemKind item)
        {
            if (item is null) return false;
            return RemoveSubItemById(item.Id);
        }

        public bool AddSubItem(ItemKind? item, int index = -1)
        {
            if (item is null)
                return false;
            if (_subItemsDictionary.ContainsKey(item.Id))
            {
                for (int i = 0; i < _subItems.Count; i++)
                {
                    if (_subItems[i].Id == item.Id)
                    {
                        _subItems[i] = item;
                        _subItemsDictionary[item.Id] = item;
                        if (index >= 0)
                        {
                            _subItems.Move(i, index);
                        }
                        return true;
                    }
                }
                throw new Exception();
            }
            _subItemsDictionary[item.Id] = item;
            if (index >= 0)
                _subItems.Insert(index, item);
            else
                _subItems.Add(item);
            return true;
        }

        public void RefreshSubItems(IList<ItemKind?> items)
        {
            if (items is null) return;
            _subItemsDictionary.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                ItemKind? item = items[i];
                if (item is null) continue;

                if (i < _subItems.Count)
                {
                    _subItems[i] = item;
                }
                else
                {
                    _subItems.Add(item);
                }
                _subItemsDictionary[item.Id] = item;
            }
            if (items.Count < _subItems.Count)
            {
                int i = _subItems.Count - 1;
                while (i >= items.Count)
                {
                    _subItems.RemoveAt(i);
                }
            }
        }

        public bool HasSubItem(ItemKind? item)
        {
            if (item is null) return false;
            return _subItemsDictionary.ContainsKey(item.Id);
        }

        public IList<ICardBindable> GetItemsOnDay(DateTime dateTime)
        {
            List<ICardBindable> res = [];
            if (!HasSchedule)
            {
                res.Add(new ItemNoDate(this));
                return res;
            }
            var l = Schedule.TimesOnDate(dateTime);
            foreach (var itemTime in l)
            {
                res.Add(new ItemWithDate(this, itemTime.Date + itemTime.Time));
            }
            return res;
        }

        public ICardBindable GetItemNoDate()
        {
            return new ItemNoDate(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
