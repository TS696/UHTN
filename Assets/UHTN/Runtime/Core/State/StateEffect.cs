using System;

namespace UHTN
{
    public readonly struct StateEffect
    {
        public readonly StateEffectOperator Operator;
        public readonly int Value;
        public readonly StateEffectType Type;

        public StateEffect(StateEffectOperator ope, int value, StateEffectType type)
        {
            Operator = ope;
            Value = value;
            Type = type;
        }

        public static StateEffect Assign<T>(T value) where T : Enum
        {
            return Assign((int)(object)value);
        }

        public static StateEffect Assign(int value, StateEffectType type = StateEffectType.PlanAndExecute)
        {
            return new StateEffect(StateEffectOperator.Assign, value, type);
        }

        public static StateEffect Assign(bool value, StateEffectType type = StateEffectType.PlanAndExecute)
        {
            return Assign(value ? 1 : 0, type);
        }

        public static StateEffect Add(int value, StateEffectType type = StateEffectType.PlanAndExecute)
        {
            return new StateEffect(StateEffectOperator.Add, value, type);
        }

        public static StateEffect Subtract(int value, StateEffectType type = StateEffectType.PlanAndExecute)
        {
            return new StateEffect(StateEffectOperator.Subtract, value, type);
        }

        public static StateEffect Multiply(int value, StateEffectType type = StateEffectType.PlanAndExecute)
        {
            return new StateEffect(StateEffectOperator.Multiply, value, type);
        }

        public static StateEffect Divide(int value, StateEffectType type = StateEffectType.PlanAndExecute)
        {
            return new StateEffect(StateEffectOperator.Divide, value, type);
        }

        public int Apply(int value)
        {
            return Operator switch
            {
                StateEffectOperator.Assign => Value,
                StateEffectOperator.Add => value + Value,
                StateEffectOperator.Subtract => value - Value,
                StateEffectOperator.Multiply => value * Value,
                StateEffectOperator.Divide => value / Value,
                StateEffectOperator.None => value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
