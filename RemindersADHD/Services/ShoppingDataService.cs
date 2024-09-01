using RemindersADHD.MVVM.Models;
using SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RemindersADHD.Services
{
    public static class ShoppingDataService
    {
        private static SQLiteAsyncConnection? db;
        [MemberNotNull("db")]
        public static async Task Init()
        {
            if (db != null) return;
            var databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "shopping-database.db");
            db = new SQLiteAsyncConnection(databasePath);
            await db.CreateTableAsync<ShoppingItem>();
        }

        public static async Task AddItem(ShoppingItem item)
        {
            await Init();
            await db.InsertAsync(item);
        }
        public static async Task RemoveItem(int id)
        {
            await Init();
            await db.DeleteAsync<ShoppingItem>(id);
        }

        public static async Task<IEnumerable<ShoppingItem>> GetItems()
        {
            await Init();
            var items = await db.Table<ShoppingItem>().ToListAsync();
            return items;
        }

        public static async Task<IEnumerable<ShoppingItem>> GetItemsWhere(Expression<Func<ShoppingItem, bool>> pred)
        {
            await Init();
            var items = db.Table<ShoppingItem>();
            items = items.Where(pred);

            return await items.ToListAsync();
        }

        public static async Task<ShoppingItem> GetItem(int id)
        {
            await Init();

            var item = await db.Table<ShoppingItem>().FirstOrDefaultAsync(c => c.Id == id);
            return item;
        }

        public static async Task UpdateItem(ShoppingItem item)
        {
            await Init();

            await db.UpdateAsync(item);
        }
    }
}
