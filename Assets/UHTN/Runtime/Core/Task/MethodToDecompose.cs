namespace UHTN
{
    public readonly struct MethodToDecompose
    {
        public readonly ValueRange SubTaskRange;
        public readonly ValueRange PreconditionRange;
        
        public MethodToDecompose(ValueRange subTaskRange, ValueRange preconditionRange)
        {
            SubTaskRange = subTaskRange;
            PreconditionRange = preconditionRange;
        }
    }
}
