namespace UHTN
{
    public readonly struct TaskAttribute
    {
        public readonly TaskType Type;

        public TaskAttribute(TaskType type)
        {
            Type = type;
        }
    }
}
