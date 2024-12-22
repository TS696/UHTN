using System;

namespace UHTN
{
    public class WsFieldEnum<T> : IWsFieldType where T : Enum
    {
        public string ToDisplayString(int value)
        {
            return Enum.GetName(typeof(T), value);
        }

        public bool Validate(int value)
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public StateComparisonOperator[] ApplicableComparisonOperators { get; } =
        {
            StateComparisonOperator.Equal,
            StateComparisonOperator.NotEqual,
        };

        public StateEffectOperator[] ApplicableEffectOperators { get; } =
        {
            StateEffectOperator.Assign,
        };
    }
}
