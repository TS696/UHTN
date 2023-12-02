namespace UHTN
{
    public readonly struct ValueRange
    {
        public readonly int Start;
        public readonly int Length;
        public int End => Start + Length;

        public ValueRange(int start, int length)
        {
            Start = start;
            Length = length;
        }
    }
}
