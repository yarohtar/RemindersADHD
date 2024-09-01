using RemindersADHD.MVVM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.ViewModels
{
    public class ItemDetailsViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private ItemKind _item;
        public ItemKind Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }
        private string _newName = "";



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query["item"] is not ItemKind item) throw new Exception();
            Item = item;
        }
    }
}
