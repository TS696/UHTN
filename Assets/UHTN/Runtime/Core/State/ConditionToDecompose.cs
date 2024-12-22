namespace UHTN
{
    public readonly struct ConditionToDecompose
    {
        public readonly int StateIndex;
        public readonly StateCondition Value;

        public ConditionToDecompose(int stateIndex, StateCondition value)
        {
            StateIndex = stateIndex;
            Value = value;
        }
    }
}
