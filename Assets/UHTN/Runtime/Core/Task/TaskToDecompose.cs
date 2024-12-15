namespace UHTN
{
    public readonly struct TaskToDecompose
    {
        public readonly int Index;
        public readonly DecompositionTiming DecompositionTiming;

        public TaskToDecompose(int index, DecompositionTiming decompositionTiming)
        {
            Index = index;
            DecompositionTiming = decompositionTiming;
        }
    }
}
