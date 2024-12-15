namespace UHTN.Agent
{
    public interface IIntSensor : ISensor<int>
    {
        int ISensor<int>.FromWorldState(int state) => state;
        int ISensor<int>.ToWorldState(int value) => value;
    }
}
