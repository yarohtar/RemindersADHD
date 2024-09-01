using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models
{
    public interface ICardBindable
    {
        public ItemKind Kind { get; }
        public ObservableCollection<ICardBindable> SubItems { get; }
        public bool Done { get; set; }
    }
}
