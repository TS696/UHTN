namespace UHTN.Agent
{
    public interface ISensor
    {
        int UpdateState(int state);
    }

    public interface ISensor<T> : ISensor
    {
        int ISensor.UpdateState(int state)
        {
            var value = FromWorldState(state);
            var nextValue = Update(value);
            return ToWorldState(nextValue);
        }

        T FromWorldState(int state);
        int ToWorldState(T value);
        T Update(T current);
    }
}
