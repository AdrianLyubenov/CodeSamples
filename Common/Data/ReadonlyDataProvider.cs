namespace KingOfDestiny.Common.Data
{
    public abstract class ReadonlyDataProvider<T> : IReadonlyDataProvider<T>
    {
        public T Data { get; }

        public ReadonlyDataProvider(T data)
        {
            Data = data;
        }
    }
}