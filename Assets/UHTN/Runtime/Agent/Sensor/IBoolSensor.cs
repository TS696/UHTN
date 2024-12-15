namespace UHTN.Agent
{
    public interface IBoolSensor : ISensor<bool>
    {
        bool ISensor<bool>.FromWorldState(int state) => state != 0;
        int ISensor<bool>.ToWorldState(bool value) => value ? 1 : 0;
    }
}
