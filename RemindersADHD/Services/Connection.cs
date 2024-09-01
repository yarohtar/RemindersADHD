namespace Connect
{
    public class Connection<T1, T2>
        where T1 : IUniqueId
        where T2 : IUniqueId
    {
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public Connection() { }
        public Connection(T1 t1, T2 t2)
        {
            ParentId = t1.Id;
            ChildId = t2.Id;
        }
    }
}
