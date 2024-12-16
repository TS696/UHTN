namespace UHTN
{
    public readonly struct TaskToDecompose
    {
        public readonly TaskType Type;
        public readonly ValueRange PreconditionRange;
        public readonly ValueRange EffectRange;
        
        public TaskToDecompose(TaskType type, ValueRange preconditionRange, ValueRange effectRange)
        {
            Type = type;
            PreconditionRange = preconditionRange;
            EffectRange = effectRange;
        }
    }
}
