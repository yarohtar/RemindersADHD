using CommunityToolkit.Maui.Media;
using RemindersADHD.MVVM.Models.Scheduling;
using RemindersADHD.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models
{
    public class ItemKind : INotifyPropertyChanged, IUniqueId, IParent<ItemKind>, IParent<Tracker>
    {
        public ItemKind()
        {
        }
        public ItemKind(Tracker tracker)
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

        [Ignore]
        public ObservableCollection<ItemKind> SubItems { get; set; } = [];

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
        public IEqualityComparer<IUniqueId> Comparer => _comparer;

        //private bool _isDone = false;
        //public bool IsDone
        //{
        //    get => _isDone;
        //    set
        //    {
        //        _isDone = value;
        //        OnPropertyChanged();
        //    }
        //}


        public void RemoveSubItemById(int id)
        {
            SubItems.Remove(SubItems.First(item => item.Id == id));
        }

        public IList<ICardBindable> GetItemsOnDay(DateTime dateTime)
        {
            List<ICardBindable> res = [];
            if (!HasSchedule) {
                res.Add(new ItemNoDate(this));
                return res;
            }
            var l = Schedule.TimesOnDate(dateTime);
            foreach(var itemTime in l)
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

        public bool HasChild(ItemKind child)
        {
            return SubItems.Contains(child, Comparer);
        }

        public bool HasChild(Tracker child)
        {
            return Tracker.Comparer.Equals(Tracker, child);
        }
    }
}
