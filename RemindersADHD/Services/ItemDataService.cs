using Connect;
using RemindersADHD.MVVM.Models;
using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace RemindersADHD.Services
{
    [Connect(typeof(ItemKind), typeof(Tracker))]
    public partial class ItemTrackerConnection { }

    [Connect(typeof(Tracker), typeof(ItemKind))]
    public partial class TrackerItemConnection { }

    [Connect(typeof(ItemKind), typeof(ItemKind))]
    public partial class ItemSubItemConnection { }

    [Connect(typeof(Tracker), typeof(ItemKind))]
    public partial class TrackerDateConnection
    {
        public DateTime DateTime { get; set; }
        public TrackerDateConnection(Tracker t, DateTime date, ItemKind item) : base(t, item) { DateTime = date; }
    }

    public static class ItemDataService
    {
        private static SQLiteAsyncConnection? db;
        [MemberNotNull("db")]
        public static async Task Init()
        {
            if (db != null) return;
            var databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "items-database.db");
            db = new SQLiteAsyncConnection(databasePath);
            await db.CreateTableAsync<ItemKind>();
            await db.CreateTableAsync<Tracker>();
            await db.CreateTableAsync<ItemTrackerConnection>(); //many to one tracker
            await db.CreateTableAsync<ItemSubItemConnection>(); //many to many subitems
            await db.CreateTableAsync<TrackerItemConnection>(); //many to many, no date tracking
            await db.CreateTableAsync<TrackerDateConnection>(); //one to many, date tracking
        }

        public static async Task DeleteEverything()
        {
            await Init();
            foreach (var tbm in db.TableMappings)
                await db.DeleteAllAsync(tbm);
        }

        private readonly static Dictionary<int, ItemKind> _loadedItems = [];
        private readonly static Dictionary<int, Tracker> _loadedTrackers = [];

        #region Queries
        private static AsyncTableQuery<TrackerItemConnection> NoDateTracking(Tracker tracker)
        {
            return db!.Table<TrackerItemConnection>().Where(conn => conn.ParentId == tracker.Id);
        }
        private static AsyncTableQuery<TrackerDateConnection> DateTracking(Tracker tracker)
        {
            return db!.Table<TrackerDateConnection>().Where(conn => conn.ParentId == tracker.Id);
        }
        #endregion

        #region Add
        public static async Task AddItem(ItemKind item)
        {
            if (_loadedItems.ContainsKey(item.Id))
                return;
            await Init();
            await db.InsertAsync(item);
            _loadedItems[item.Id] = item;
            await AddTracker(item.Tracker);
            await db.InsertAsync(new ItemTrackerConnection(item, item.Tracker));

            foreach (ItemKind subitem in item.SubItems)
            {
                //await db.InsertAsync(new ItemSubItemConnection(item, subitem));
                await AddSubItem(item, subitem);
            }
        }

        public static async Task AddSubItem(ItemKind item, ItemKind subitem)
        {
            await Init();
            if (!_loadedItems.ContainsKey(subitem.Id))
            {
                await AddItem(subitem);
            }
            await db.InsertAsync(new ItemSubItemConnection(item, subitem));
        }

        public static async Task AddTracker(Tracker tracker)
        {
            await Init();
            if (await db.UpdateAsync(tracker) == 0)
            {
                await db.InsertAsync(tracker);
            }
            await NoDateTracking(tracker).DeleteAsync();
            foreach (ItemKind item in tracker.NoDateTracking)
            {
                await AddNoDateDone(tracker, item);
            }
            await DateTracking(tracker).DeleteAsync();
            foreach (var date in tracker.DatesTracked)
            {
                foreach (ItemKind item in tracker[date])
                {
                    await AddDateDone(tracker, date, item);
                }
            }
            _loadedTrackers[tracker.Id] = tracker;
        }

        public static async Task AddDateDone(Tracker t, DateTime date, ItemKind item)
        {
            await Init();
            if (await db.Table<TrackerDateConnection>()
                .Where(tdc => tdc.ParentId == t.Id)
                .Where(tdc => tdc.ChildId == item.Id)
                .Where(tdc => tdc.DateTime == date)
                .CountAsync() > 0)
                return;
            await db.InsertAsync(new TrackerDateConnection(t, date, item));
        }

        public static async Task AddNoDateDone(Tracker t, ItemKind item)
        {
            await Init();
            await db.InsertAsync(new TrackerItemConnection(t, item));
        }

        #endregion

        #region Get
        private static async Task RefreshSubItems(ItemKind item)
        {
            var l = await db!.Table<ItemSubItemConnection>().ToListAsync();
            IList<ItemKind?> subitems = [];
            foreach (var connection in l)
            {
                ItemKind? subitem = await GetItem(connection.ChildId);
                if (subitem is null)
                {
                    await db!.Table<ItemSubItemConnection>()
                        .Where(conn => conn.ParentId == item.Id)
                        .Where(conn => conn.ChildId == connection.ChildId)
                        .DeleteAsync();
                    continue;
                }
                subitems.Add(subitem);
            }
            item.RefreshSubItems(subitems);
        }
        public static async Task RefreshItem(ItemKind item)
        {
            await Init();
            if (item is null) return;
            item.Tracker = await GetTrackerForItem(item);
            await RefreshSubItems(item);
        }
        public static async Task<ItemKind?> GetItem(int itemId)
        {
            await Init();
            if (_loadedItems.ContainsKey(itemId))
                return _loadedItems[itemId];

            ItemKind item = await db.Table<ItemKind>().Where(i => i.Id == itemId).FirstOrDefaultAsync();
            if (item is null) return null;

            item.Tracker = await GetTrackerForItem(item);
            await RefreshSubItems(item);

            _loadedItems[itemId] = item;

            return item;
        }
        public static async Task<Tracker?> GetTracker(int trackerId)
        {
            await Init();
            if (_loadedTrackers.ContainsKey(trackerId))
                return _loadedTrackers[trackerId];
            Tracker t = await db.Table<Tracker>().Where(tr => tr.Id == trackerId).FirstOrDefaultAsync();
            if (t is null) return null;

            var noDateTracking = await NoDateTracking(t).ToArrayAsync();
            foreach (var c in noDateTracking)
            {
                t.NoDateTracking.Add(await GetItem(c.ChildId) ?? throw new Exception());
            }
            var dateTracking = await DateTracking(t).ToArrayAsync();
            foreach (var c in dateTracking)
            {
                ItemKind? childItem = await GetItem(c.ChildId);
                if (childItem is null)
                {
                    await RemoveDateDone(t, c.DateTime, new ItemKind { Id = c.ChildId });
                    continue;
                }
                t[c.DateTime].Add(childItem);
            }
            _loadedTrackers[trackerId] = t;
            return t;
        }
        private static async Task<Tracker> GetTrackerForItem(ItemKind item)
        {
            var conn = await db!.Table<ItemTrackerConnection>().Where(cn => cn.ParentId == item.Id).FirstOrDefaultAsync();
            if (conn is null)
            {
                Tracker tnew = new Tracker();
                await AddTracker(tnew);
                await db!.InsertAsync(new ItemTrackerConnection { ParentId = item.Id, ChildId = tnew.Id });
                return tnew;
            }
            Tracker? t = await GetTracker(conn.ChildId) ?? throw new Exception();
            return t;
        }
        public static async Task<IList<ItemKind>> GetItems()
        {
            await Init();
            var items = await db.Table<ItemKind>().ToListAsync();

            foreach (var item in items)
            {
                if (!_loadedItems.ContainsKey(item.Id))
                    _loadedItems[item.Id] = item;
            }

            foreach (var item in items)
            {
                item.Tracker = await GetTrackerForItem(item);
                await RefreshSubItems(item);
            }
            return items;
        }

        public static async Task<IList<Tracker>> GetTrackers()
        {
            await Init();
            var trackers = await db.Table<Tracker>().ToListAsync();
            var res = new List<Tracker>();
            foreach (var tracker in trackers)
            {
                Tracker? t = await GetTracker(tracker.Id);
                if (t is null)
                {
                    await RemoveTracker(tracker.Id);
                    continue;
                }
                res.Add(t);
            }

            return res;
        }

        #endregion

        #region Remove
        public static async Task RemoveItem(int id)
        {
            await Init();
            await db.DeleteAsync<ItemKind>(id);

            await db.Table<ItemSubItemConnection>().Where(conn => conn.ParentId == id).DeleteAsync();
            await db.Table<ItemSubItemConnection>().Where(conn => conn.ChildId == id).DeleteAsync();
            await db.Table<ItemTrackerConnection>().Where(conn => conn.ParentId == id).DeleteAsync();
            await db.Table<TrackerItemConnection>().Where(conn => conn.ChildId == id).DeleteAsync();
            await db.Table<TrackerDateConnection>().Where(conn => conn.ChildId == id).DeleteAsync();

            _loadedItems.Remove(id);
            foreach (var item in _loadedItems.Values)
            {
                item.RemoveSubItemById(id);
            }
            foreach (Tracker t in _loadedTrackers.Values)
            {
                t.RemoveItemById(id);
            }
        }

        private static async Task RemoveTracker(int id)
        {
            await Init();
            if (await db.Table<ItemTrackerConnection>().Where(conn => conn.ChildId == id).CountAsync() > 0
                || _loadedItems.Where(kvp => kvp.Value.Tracker.Id == id).Any())
            {
                throw new Exception($@"Table count is {await db.Table<ItemTrackerConnection>().Where(conn => conn.ChildId == id).CountAsync()}
                                    loaded items count is {_loadedItems.Where(kvp => kvp.Value.Tracker.Id == id).Count()}");
            }
            await db.DeleteAsync<Tracker>(id);
            await db.Table<TrackerItemConnection>().Where(conn => conn.ParentId == id).DeleteAsync();
            await db.Table<TrackerDateConnection>().Where(conn => conn.ParentId == id).DeleteAsync();
            _loadedTrackers.Remove(id);
        }

        public static async Task FilterTrackers()
        {
            await Init();
            foreach (Tracker t in await GetTrackers())
            {
                if (await db.Table<ItemTrackerConnection>().Where(conn => conn.ChildId == t.Id).CountAsync() == 0)
                    await RemoveTracker(t.Id);
            }
        }

        public static async Task RemoveDateDone(ItemWithDate item)
        {
            await RemoveDateDone(item.Kind.Tracker, item.DateTime, item.Kind);
        }

        public static async Task RemoveDateDone(Tracker t, DateTime date, ItemKind item)
        {
            await Init();
            await DateTracking(t)
                .Where(conn => conn.ChildId == item.Id)
                .Where(conn => conn.DateTime == date)
                .DeleteAsync();
        }

        public static async Task RemoveNoDateDone(ICardBindable item)
        {
            await RemoveNoDateDone(item.Kind.Tracker, item.Kind);
        }
        public static async Task RemoveNoDateDone(Tracker t, ItemKind item)
        {
            await Init();
            await NoDateTracking(t)
                .Where(conn => conn.ChildId == item.Id)
                .DeleteAsync();
        }

        #endregion

        #region Update
        public static async Task UpdateItem(ItemKind item)
        {
            await Init();

            await db.UpdateAsync(item);
        }

        public static async Task UpdateTrackerInItem(ItemKind item)
        {
            await Init();
            await db.Table<ItemTrackerConnection>().Where(conn => conn.ParentId == item.Id).DeleteAsync();
            await db.InsertAsync(new ItemTrackerConnection(item, item.Tracker));
        }
        #endregion
    }
}
