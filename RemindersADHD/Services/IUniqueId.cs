using SQLite;

namespace Connect
{
    public interface IUniqueId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
