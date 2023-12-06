using System;

namespace KingOfDestiny.Common.Data
{
    public abstract class DataProvider<T> : IReadonlyDataProvider<T>
    {
        public T Data { get; protected set; }

        public event Action<T> DataUpdated;
        
        public void SetData(T data)
        {
            Data = data;
            
            DataUpdated?.Invoke(Data);
        }
    }
}