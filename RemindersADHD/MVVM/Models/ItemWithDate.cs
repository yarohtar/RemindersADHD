using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models
{
    public class ItemWithDate : INotifyPropertyChanged, ICardBindable
    {
        public ItemKind Kind { get; private set; }
        /// <summary>
        /// Instance's DateTime
        /// </summary>
        public DateTime DateTime { get; private set; }
        public ObservableCollection<ICardBindable> SubItems { get; set; } = [];
        public bool Done { get => Kind.Tracker[DateTime].Contains(Kind);
            set {
                if (value)
                    Kind.Tracker[DateTime].Add(Kind);
                else
                    Kind.Tracker[DateTime].Remove(Kind);
                OnPropertyChanged(nameof(Done));
            }
        }


        public ItemWithDate(ItemKind itemKind, DateTime dateTime)
        {
            Kind = itemKind;
            DateTime = dateTime;
            foreach(ItemKind itemk in Kind.SubItems)
            {
                var list = itemk.GetItemsOnDay(DateTime);
                foreach(var itemd in list)
                {
                    SubItems.Add(itemd);
                }
            }
        }

        public ItemWithDate(ItemKind itemKind)
        {
            Kind = itemKind;
            foreach(ItemKind itemk in Kind.SubItems)
            {
                SubItems.Add(itemk.GetItemNoDate());
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
