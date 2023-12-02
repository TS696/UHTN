namespace UHTN
{
    public readonly struct StateCondition
    {
        public readonly StateComparisonOperator Operator;
        public readonly int Value;

        public StateCondition(StateComparisonOperator ope, int value)
        {
            Operator = ope;
            Value = value;
        }

        public static StateCondition Equal(int value)
        {
            return new StateCondition(StateComparisonOperator.Equal, value);
        }

        public static StateCondition Equal<T>(T value) where T : System.Enum
        {
            return Equal((int)(object)value);
        }

        public static StateCondition Equal(bool value)
        {
            return Equal(value ? 1 : 0);
        }

        public static StateCondition NotEqual(int value)
        {
            return new StateCondition(StateComparisonOperator.NotEqual, value);
        }

        public static StateCondition NotEqual<T>(T value) where T : System.Enum
        {
            return NotEqual((int)(object)value);
        }

        public static StateCondition NotEqual(bool value)
        {
            return NotEqual(value ? 1 : 0);
        }

        public static StateCondition GreaterThan(int value)
        {
            return new StateCondition(StateComparisonOperator.GreaterThan, value);
        }

        public static StateCondition GreaterThan<T>(T value) where T : System.Enum
        {
            return GreaterThan((int)(object)value);
        }

        public static StateCondition GreaterThanOrEqual(int value)
        {
            return new StateCondition(StateComparisonOperator.GreaterThanOrEqual, value);
        }

        public static StateCondition GreaterThanOrEqual<T>(T value) where T : System.Enum
        {
            return GreaterThanOrEqual((int)(object)value);
        }

        public static StateCondition LessThan(int value)
        {
            return new StateCondition(StateComparisonOperator.LessThan, value);
        }

        public static StateCondition LessThan<T>(T value) where T : System.Enum
        {
            return LessThan((int)(object)value);
        }

        public static StateCondition LessThanOrEqual(int value)
        {
            return new StateCondition(StateComparisonOperator.LessThanOrEqual, value);
        }

        public static StateCondition LessThanOrEqual<T>(T value) where T : System.Enum
        {
            return LessThanOrEqual((int)(object)value);
        }

        public bool Check(int value)
        {
            return Operator switch
            {
                StateComparisonOperator.None => true,
                StateComparisonOperator.Equal => value == Value,
                StateComparisonOperator.NotEqual => value != Value,
                StateComparisonOperator.GreaterThan => value > Value,
                StateComparisonOperator.GreaterThanOrEqual => value >= Value,
                StateComparisonOperator.LessThan => value < Value,
                StateComparisonOperator.LessThanOrEqual => value <= Value,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
    }
}
