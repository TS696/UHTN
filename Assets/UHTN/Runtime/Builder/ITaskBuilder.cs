namespace UHTN.Builder
{
    public interface ITaskBuilder
    {
        ITask Task { get; }
        DecompositionTiming DecompositionTiming { get; }
    }
}
