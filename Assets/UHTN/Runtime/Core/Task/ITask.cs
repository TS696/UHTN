namespace UHTN
{
    public interface ITask
    {
        TaskAttribute Attribute { get; }
        string Name { get; }
    }
}
