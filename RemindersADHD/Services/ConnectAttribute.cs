namespace Connect
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class ConnectAttribute : Attribute
    {
        public ConnectAttribute(Type t1, Type t2)
        {
            LeftName = t1.Name;
            RightName = t2.Name;
        }

        public string LeftName { get; set; }
        public string RightName { get; set; }
    }
}