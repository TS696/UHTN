namespace UHTN.Builder
{
    public class FixedTaskBuilder : ITaskBuilder
    {
        public ITask Task { get; }
        public DecompositionTiming DecompositionTiming { get; }

        public FixedTaskBuilder(ITask task, DecompositionTiming decompositionTiming)
        {
            Task = task;
            DecompositionTiming = decompositionTiming;
        }
    }
}
