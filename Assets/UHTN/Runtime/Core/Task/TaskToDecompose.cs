namespace UHTN
{
    public readonly struct TaskToDecompose
    {
        public readonly TaskType Type;
        public readonly ValueRange PreConditionRange;
        public readonly ValueRange EffectRange;
        
        public TaskToDecompose(TaskType type, ValueRange preConditionRange, ValueRange effectRange)
        {
            Type = type;
            PreConditionRange = preConditionRange;
            EffectRange = effectRange;
        }
    }
}
