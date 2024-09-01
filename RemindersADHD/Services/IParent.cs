namespace RemindersADHD.Services
{
    public interface IParent<T>
    {
        public bool HasChild(T child);
    }
}
