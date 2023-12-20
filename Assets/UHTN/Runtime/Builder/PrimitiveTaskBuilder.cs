using System;

namespace UHTN.Builder
{
    public class PrimitiveTaskBuilder<T> : ITaskBuilder where T : Enum
    {
        private readonly TaskBuildHelper<T> _helper;
        private readonly PrimitiveTask _primitiveTask;

        ITask ITaskBuilder.Task => _primitiveTask;

        public PrimitiveTaskBuilder(TaskBuildHelper<T> helper, PrimitiveTask primitiveTask)
        {
            _helper = helper;
            _primitiveTask = primitiveTask;
        }

        public PrimitiveTaskBuilder<T> Precondition(T type, StateCondition condition)
        {
            _helper.SetCondition(_primitiveTask.PreConditions, type, condition);
            return this;
        }

        public PrimitiveTaskBuilder<T> Effect(T type, StateEffect effect)
        {
            _helper.SetEffect(_primitiveTask.Effects, type, effect);
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
