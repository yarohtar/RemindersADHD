using System.Diagnostics.CodeAnalysis;

namespace Connect
{
    public class IdEqualityComparer<T> : IEqualityComparer<IUniqueId> where T : IUniqueId
    {
        public bool Equals(IUniqueId? x, IUniqueId? y)
        {
            if (x is not T item1) return false;
            if (y is not T item2) return false;
            return item1.Id == item2.Id;
        }

        public int GetHashCode([DisallowNull] IUniqueId obj)
        {
            if (obj is not T) throw new Exception();
            return obj.Id.GetHashCode();
        }
    }
}
