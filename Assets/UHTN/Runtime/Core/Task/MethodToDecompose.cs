namespace UHTN
{
    public readonly struct MethodToDecompose
    {
        public readonly ValueRange SubTaskRange;
        public readonly ValueRange PreConditionRange;
        
        public MethodToDecompose(ValueRange subTaskRange, ValueRange preConditionRange)
        {
            SubTaskRange = subTaskRange;
            PreConditionRange = preConditionRange;
        }
    }
}
