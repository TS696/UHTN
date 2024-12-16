namespace UHTN
{
    public readonly struct EffectToDecompose
    {
        public readonly int StateIndex;
        public readonly StateEffect Value;

        public EffectToDecompose(int stateIndex, StateEffect value)
        {
            StateIndex = stateIndex;
            Value = value;
        }
    }
}
