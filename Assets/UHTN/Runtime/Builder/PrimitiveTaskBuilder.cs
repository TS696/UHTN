using System;

namespace UHTN.Builder
{
    public class PrimitiveTaskBuilder<T> : ITaskBuilder where T : Enum
    {
        private readonly PrimitiveTask _primitiveTask;

        ITask ITaskBuilder.Task => _primitiveTask;
        DecompositionTiming ITaskBuilder.DecompositionTiming => DecompositionTiming.Immediate;

        public PrimitiveTaskBuilder(PrimitiveTask primitiveTask)
        {
            _primitiveTask = primitiveTask;
        }

        public PrimitiveTaskBuilder<T> Precondition(T type, StateCondition condition)
        {
            _primitiveTask.Preconditions.Add(new ConditionToDecompose((int)(object)type, condition));
            return this;
        }

        public PrimitiveTaskBuilder<T> Effect(T type, StateEffect effect)
        {
            _primitiveTask.Effects.Add(new EffectToDecompose((int)(object)type, effect));
            return this;
        }

        public PrimitiveTaskBuilder<T> Operator(Action action)
        {
            var op = new FunctionOperator(action, () => OperatorState.Success, null);
            return Operator(op);
        }

        public PrimitiveTaskBuilder<T> Operator(Func<OperatorState> func)
        {
            var op = new FunctionOperator(null, func, null);
            _primitiveTask.SetOperator(op);
            return this;
        }

        public PrimitiveTaskBuilder<T> Operator(IOperator op)
        {
            _primitiveTask.SetOperator(op);
            return this;
        }

        public PrimitiveTaskBuilder<T> PreExecute(Action action)
        {
            _primitiveTask.PreExecute += action;
            return this;
        }

        public PrimitiveTaskBuilder<T> PostExecute(Action action)
        {
            _primitiveTask.PostExecute += action;
            return this;
        }
    }
}
