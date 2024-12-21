namespace UHTN
{
    public class WsFieldInt : IWsFieldType
    {
        public static WsFieldInt Instance { get; } = new();

        public string ToDisplayString(int value)
        {
            return value.ToString();
        }

        public bool Validate(int value)
        {
            return true;
        }

        public StateComparisonOperator[] ApplicableComparisonOperators { get; } = 
        {
            StateComparisonOperator.Equal,
            StateComparisonOperator.NotEqual,
            StateComparisonOperator.GreaterThan,
            StateComparisonOperator.GreaterThanOrEqual,
            StateComparisonOperator.LessThan,
            StateComparisonOperator.LessThanOrEqual
        };

        public StateEffectOperator[] ApplicableEffectOperators { get; } =
        {
            StateEffectOperator.Assign,
            StateEffectOperator.Add,
            StateEffectOperator.Subtract,
            StateEffectOperator.Multiply,
            StateEffectOperator.Divide
        };
    }
}
