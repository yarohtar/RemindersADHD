namespace RemindersADHD.Services
{
    public class Connection<T1, T2> 
        where T1: IUniqueId//, IParent<T2>
        where T2: IUniqueId
    {
        public int ParentId {  get; set; }
        public int ChildId { get; set; }
        public string TableName => "Connection";
        public Connection() { }
        public Connection(T1 t1, T2 t2)
        {
            //if(!t1.HasChild(t2))
            //{
            //    throw new Exception();
            //}
            ParentId = t1.Id;
            ChildId = t2.Id;
        }
    }
}
