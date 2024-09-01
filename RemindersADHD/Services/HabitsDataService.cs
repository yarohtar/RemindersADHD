using RemindersADHD.MVVM.Models;
using SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RemindersADHD.Services
{
    public static class HabitsDataService
    {

        private static SQLiteAsyncConnection? db;

        [MemberNotNull(nameof(db))]
        public static async Task Init()
        {
            if (db != null) return;
            var databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "habits-database.db");
            db = new SQLiteAsyncConnection(databasePath);
            await db.CreateTableAsync<Habit>();
            await db.CreateTableAsync<DateDone>();
        }

        private static AsyncTableQuery<DateDone> DatesQuery(Habit habit)
        {
            return db!.Table<DateDone>().Where(d => d.ExternalId == habit.Id);
        }
        private static AsyncTableQuery<DateDone> DatesQuery(int habitId)
        {
            return db!.Table<DateDone>().Where(d => d.ExternalId == habitId);
        }

        public static async Task AddItem(Habit item)
        {
            await Init();
            await db.InsertAsync(item);
            await RemoveDates(item);
            await db.InsertAllAsync(item.datesCompleted.Select(d => new DateDone { Date = d, ExternalId = item.Id }));
        }
        public static async Task RemoveItem(int id)
        {
            await Init();
            await db.DeleteAsync<Habit>(id);
            await DatesQuery(id).DeleteAsync();
        }
        public static async Task AddDate(int itemId, DateTime date)
        {
            await Init();
            if (await GetItem(itemId) is null)
                return;
            await db.InsertAsync(new DateDone { ExternalId = itemId, Date = date });
        }
        public static async Task RemoveDate(int itemId, DateTime date)
        {
            await Init();
            await DatesQuery(itemId).DeleteAsync(d => d.Date == date);
        }


        public static async Task UpdateDatesInHabit(Habit habit)
        {
            if (habit is null) return;
            await Init();
            var l = (await DatesQuery(habit).ToListAsync()).Select(d => d.Date);
            habit.datesCompleted = new HashSet<DateTime>(l);
        }
        private static async Task<IEnumerable<Habit>> GetItems(AsyncTableQuery<Habit> query)
        {
            var items = await query.ToListAsync();
            foreach (var item in items)
            {
                await UpdateDatesInHabit(item);
            }
            return items;
        }
        public static async Task<IEnumerable<Habit>> GetItems()
        {
            await Init();
            return await GetItems(db.Table<Habit>());
        }

        public static async Task<IEnumerable<Habit>> GetItemsWhere(Expression<Func<Habit, bool>> pred)
        {
            await Init();
            return await GetItems(db.Table<Habit>().Where(pred));
        }
        public static async Task<Habit> GetItem(int id)
        {
            await Init();

            var item = await db.Table<Habit>().FirstOrDefaultAsync(c => c.Id == id) ?? throw new Exception();
            await UpdateDatesInHabit(item);
            return item;
        }



        public static async Task UpdateItemAndDates(Habit item)
        {
            if (item is null) return;
            await Init();
            await UpdateItem(item);
            await UpdateDatesInDatabase(item);
        }
        public static async Task UpdateItem(Habit item)
        {
            if (item is null) return;
            await Init();
            await db.UpdateAsync(item);
        }
        public static async Task RemoveDates(Habit habit)
        {
            if (habit is null) return;
            await Init();
            await DatesQuery(habit).DeleteAsync();
        }
        public static async Task UpdateDatesInDatabase(Habit habit) //try not to use lol
        {
            if (habit is null) return;
            await Init();
            var oldDates = (await DatesQuery(habit).ToListAsync()).Select(d => d.Date);

            var toRemove = oldDates.Except(habit.datesCompleted);
            var toAdd = habit.datesCompleted.Except(oldDates);

            await db.InsertAllAsync(toAdd);
            foreach (var dt in toRemove)
            {
                await DatesQuery(habit).DeleteAsync(d => d.Date == dt);
            }
        }
    }
}
