namespace UHTN.Builder
{
    public class FixedTaskBuilder : ITaskBuilder
    {
        public ITask Task { get; }

        public FixedTaskBuilder(ITask task)
        {
            Task = task;
        }
    }
}
