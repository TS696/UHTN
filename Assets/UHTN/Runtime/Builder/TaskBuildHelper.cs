using System;

namespace UHTN.Builder
{
    public class TaskBuildHelper<T> where T : Enum
    {
        private readonly int _stateLength;

        public TaskBuildHelper()
        {
            var values = Enum.GetValues(typeof(T));
            _stateLength = values.Length;
        }

        public PrimitiveTask CreatePrimitive(string taskName)
        {
            return new PrimitiveTask(taskName, _stateLength);
        }

        public CompoundTask CreateCompound(string taskName = "",
            DecompositionTiming decompositionTiming = DecompositionTiming.Immediate)
        {
            return new CompoundTask(taskName, decompositionTiming);
        }

        public WrappedCompoundTask CreateCompoundWrapped(string taskName, CompoundTask source,
            DecompositionTiming decompositionTiming)
        {
            var compoundTask = new WrappedCompoundTask(taskName, source, decompositionTiming);
            return compoundTask;
        }

        public Method CreateMethod()
        {
            return new Method(_stateLength);
        }

        public void SetCondition(StateCondition[] states, T type, StateCondition value)
        {
            states[(int)(object)type] = value;
        }

        public void SetEffect(StateEffect[] states, T type, StateEffect value)
        {
            states[(int)(object)type] = value;
        }
    }
}
