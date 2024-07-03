using System;

namespace UHTN.Builder
{
    public class MethodBuilder<T> where T : Enum
    {
        public Method Method => _method;

        private readonly Method _method;

        public MethodBuilder(Method method)
        {
            _method = method;
        }

        public MethodBuilder<T> Precondition(T type, StateCondition value)
        {
            _method.PreConditions[(int)(object)type] = value;
            return this;
        }

        public MethodBuilder<T> SubTasks(params ITaskBuilder[] taskBuilders)
        {
            foreach (var taskBuilder in taskBuilders)
            {
                _method.SubTasks.Add(taskBuilder.Task);
            }

            return this;
        }
    }
}
