namespace CARVES.Data
{
    public interface IDataElement : IIdElement, INameElement
    {
    }
    public interface IIdElement
    {
        public int Id { get; }
    }

    public interface INameElement
    {
        public string Name { get; }
    }
}