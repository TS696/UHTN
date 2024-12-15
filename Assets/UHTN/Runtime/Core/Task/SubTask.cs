namespace UHTN
{
    public class SubTask
    {
        public readonly ITask Task;
        public readonly DecompositionTiming DecompositionTiming;

        public SubTask(ITask task, DecompositionTiming decompositionTiming)
        {
            Task = task;
            DecompositionTiming = decompositionTiming;
        }
    }
}
