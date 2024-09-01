using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemindersADHD.MVVM.Models
{
    public class ItemNoDate : INotifyPropertyChanged, ICardBindable
    {
        public ItemKind Kind { get; private set; }

        public ObservableCollection<ICardBindable> SubItems { get; private set; } = [];

        private Tracker Tracker => Kind.Tracker;
        private HashSet<ItemKind> TrackerSet => Kind.Tracker.NoDateTracking;

        public bool Done
        {
            get => Kind.Tracker.NoDateTracking.Contains(Kind);
            set
            {
                if (value)
                {
                    TrackerSet.Add(Kind);
                    Tracker[DateTime.Now].Add(Kind);
                }
                else
                {
                    TrackerSet.Remove(Kind);
                }

                OnPropertyChanged();
            }
        }

        public ItemNoDate(ItemKind kind)
        {
            Kind = kind;
            foreach (ItemKind k in Kind.SubItems)
            {
                var l = k.GetItemsOnDay(DateTime.Now);
                foreach (var sub in l)
                    SubItems.Add(sub);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}