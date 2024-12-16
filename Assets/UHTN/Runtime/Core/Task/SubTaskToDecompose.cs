namespace UHTN
{
    public readonly struct SubTaskToDecompose
    {
        public readonly int Index;
        public readonly DecompositionTiming DecompositionTiming;

        public SubTaskToDecompose(int index, DecompositionTiming decompositionTiming)
        {
            Index = index;
            DecompositionTiming = decompositionTiming;
        }
    }
}
