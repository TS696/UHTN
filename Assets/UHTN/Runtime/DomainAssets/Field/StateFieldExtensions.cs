using System;

namespace UHTN.DomainAssets
{
    public static class StateFieldExtensions
    {
        public static string ToDisplayString(this StateComparisonOperator self)
        {
            return self switch
            {
                StateComparisonOperator.None => "",
                StateComparisonOperator.Equal => "==",
                StateComparisonOperator.NotEqual => "!=",
                StateComparisonOperator.LessThan => "<",
                StateComparisonOperator.LessThanOrEqual => "<=",
                StateComparisonOperator.GreaterThan => ">",
                StateComparisonOperator.GreaterThanOrEqual => ">=",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static string ToDisplayString(this StateEffectOperator self)
        {
            return self switch
            {
                StateEffectOperator.None => "",
                StateEffectOperator.Add => "+=",
                StateEffectOperator.Subtract => "-=",
                StateEffectOperator.Multiply => "*=",
                StateEffectOperator.Divide => "\u2215=",
                StateEffectOperator.Assign => "=",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
