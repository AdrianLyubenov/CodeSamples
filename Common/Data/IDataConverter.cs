namespace KingOfDestiny.Common.Data
{
    public interface IDataConverter<in TIn, out TOut>
    {
        TOut Convert(TIn target);
    }
}