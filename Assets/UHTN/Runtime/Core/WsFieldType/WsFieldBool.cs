namespace UHTN
{
    public class WsFieldBool : IWsFieldType
    {
        public string ToDisplayString(int value)
        {
            var flag = value >= 1;
            return flag.ToString();
        }

        public bool Validate(int value)
        {
            return value is 0 or 1;
        }

        public StateComparisonOperator[] ApplicableComparisonOperators { get; } = 
        {
            StateComparisonOperator.Equal,
            StateComparisonOperator.NotEqual
        };

        public StateEffectOperator[] ApplicableEffectOperators { get; } =
        {
            StateEffectOperator.Assign
        };
    }
}
