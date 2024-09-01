using System.Collections.ObjectModel;

namespace RemindersADHD.MVVM.Models
{
    public interface ICardBindable
    {
        public ItemKind Kind { get; }
        public ObservableCollection<ICardBindable> SubItems { get; }
        public bool Done { get; set; }
    }
}
