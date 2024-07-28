using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models
{
    public class ShoppingItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public bool IsOnList {get; set; }
        public DateTime LastBought { get; set; }
        //public List<DateTime> DatesBought { get; set; }
        public int TimesBought { get; set; }
        public int NeedToBuy { get; set; }
        public bool HasNote { get; set; }
        public string Note { get; set; } = "";

        string buyAgainDaysString = "";
        public string BuyAgainDaysString { 
            get { return buyAgainDaysString; }
            set
            {
                string filter = new string(value.Where(char.IsDigit).ToArray());
                filter.TrimStart('0');
                buyAgainDaysString = filter;
                OnPropertyChanged();
                OnPropertyChanged("BuyAgainDays");
                OnPropertyChanged("BuyOverdue");
                OnPropertyChanged("LastBoughtColour");
            }
        }
        public int BuyAgainDays
        {
            get => BuyAgainDaysString.Length>0 ? int.Parse(BuyAgainDaysString) : 0;
        }

        public bool BuyOverdue { get => BuyAgainDays > 0 && LastBought.AddDays(BuyAgainDays).CompareTo(DateTime.Now) < 0; }

        public string LastBoughtColour { get => BuyOverdue ? "Red" : "LightGray"; }

        public string LastBoughtDate
        {
            get
            {
                if (TimesBought==0) return "Never bought";
                if (LastBought.Date == DateTime.Now.Date) return "Last bought today";
                if (LastBought.Date == DateTime.Now.AddDays(-1).Date) return "Last bought yesterday";
                for(int i=2; i<=9; i++)
                {
                    if (LastBought.Date == DateTime.Now.AddDays(-i).Date) return $"Last bought {i} days ago";
                }
                return "Last bought on: " + LastBought.Date.ToString("D");
            }
        }


        public void HasNoteSwitch()
        {
            if (HasNote) HasNote = false;
            else HasNote = true;
            OnPropertyChanged("HasNote");
        }
        public ShoppingItem() { }
        public ShoppingItem(string name, int needToBuy = 1)
        {
            Name = name;
            TimesBought = 0;
            NeedToBuy = needToBuy;
            //DatesBought = new List<DateTime>();
            HasNote= false;
            Note = "";
            BuyAgainDaysString = "";
        }

        public void Buy(int amountBought = 1)
        {
            DateTime now = DateTime.Now;
            LastBought = now;
            //DatesBought.Add(now);
            TimesBought+=amountBought;
            NeedToBuy-=amountBought;
            if (NeedToBuy < 0)
            {
                NeedToBuy = 0;
            }
        }

        public static int Compare(ShoppingItem x, ShoppingItem y)
        {
            if (x.BuyOverdue && !y.BuyOverdue)
                return -1;
            if (!x.BuyOverdue && y.BuyOverdue)
                return 1;
            if (x.BuyOverdue && y.BuyOverdue)
                return y.BuyAgainDays.CompareTo(x.BuyAgainDays);
            return y.TimesBought.CompareTo(x.TimesBought);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
