namespace UHTN
{
    public interface ITask
    {
        TaskType Type { get; }
        string Name { get; }
    }
}
