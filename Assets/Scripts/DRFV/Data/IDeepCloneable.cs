namespace DRFV.Data
{
    public interface IDeepCloneable<T> where T : class
    {
        public T DeepClone();
    }
}