using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.MVVM.Models
{
    public class DateDone
    {
        public int ExternalId { get; set; }
        private DateTime date;
        public DateTime Date { get => date; set { date = value.Date; } }

        //public static implicit operator DateTime(DateDone dateDone) => dateDone.Date;

    }
}
