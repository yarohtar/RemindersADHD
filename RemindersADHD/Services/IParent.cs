using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemindersADHD.Services
{
    public interface IParent<T>
    {
        public bool HasChild(T child);
    }
}
