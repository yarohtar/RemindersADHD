using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.Services
{
    public interface IUniqueId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public IEqualityComparer<IUniqueId> Comparer { get; }
    }
}
