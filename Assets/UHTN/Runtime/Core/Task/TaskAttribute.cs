namespace UHTN
{
    public readonly struct TaskAttribute
    {
        public readonly TaskType Type;
        public readonly DecompositionTiming DecompositionTiming;

        public TaskAttribute(TaskType type, DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            Type = type;
            DecompositionTiming = decompositionTiming;
        }
    }
}
