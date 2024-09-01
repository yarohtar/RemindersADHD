using Connect;
using SQLite;

namespace RemindersADHD.MVVM.Models
{
    public class Tracker : IUniqueId
    {
        private SortedList<DateTime, HashSet<ItemKind>> _dateTracking = [];
        private HashSet<ItemKind> _noDateTracking = new(ItemKind.Comparer);

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        //private static readonly IdEqualityComparer<Tracker> _comparer = new();
        //[Ignore]
        //public IEqualityComparer<IUniqueId> Comparer => _comparer;

        [Ignore]
        public HashSet<ItemKind> this[DateTime dateTime]
        {
            get
            {
                if (!_dateTracking.Keys.Contains(dateTime))
                    _dateTracking[dateTime] = new(ItemKind.Comparer);
                return _dateTracking.AsReadOnly()[dateTime];
            }
        }
        [Ignore]
        public IList<DateTime> DatesTracked => _dateTracking.Keys;

        [Ignore]
        public HashSet<ItemKind> NoDateTracking
        {
            get => _noDateTracking;
        }

        public void RemoveItemById(int id)
        {
            NoDateTracking.Remove(NoDateTracking.First(item => item.Id == id));
            foreach (var date in DatesTracked)
            {
                this[date].Remove(this[date].First(item => item.Id == id));
            }
        }

        public int TimesDoneInInterval(DateTime start, DateTime end)
        {
            if (_dateTracking.Count == 0) { return 0; }
            if (end < start) return 0;
            return FindIndexBefore(end) - FindIndexBefore(start);
        }
        public int TimesDoneInDay(DateTime date)
        {
            return TimesDoneInInterval(date.Date, date.Date.AddDays(1));
        }
        public int TimesDoneToday()
        {
            return TimesDoneInDay(DateTime.Now);
        }
        private int FindIndexBefore(DateTime dateTime)
        {
            if (_dateTracking.Count == 0) { return -1; }
            return FindIndexBefore(dateTime, 0, _dateTracking.Count - 1);

        }
        private int FindIndexBefore(DateTime dateTime, int l, int r)
        {
            if (l >= r)
            {
                return r;
            }
            int m = (l + r) / 2;
            if (_dateTracking.Keys[m] < dateTime) { return FindIndexBefore(dateTime, m + 1, r); }
            return FindIndexBefore(dateTime, l, m);
        }
    }
}
