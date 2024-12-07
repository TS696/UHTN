using System;

namespace UHTN.Builder
{
    public class MethodBuilder<T> where T : Enum
    {
        public Method Method { get; }

        public MethodBuilder(Method method)
        {
            Method = method;
        }

        public MethodBuilder<T> Precondition(T type, StateCondition value)
        {
            Method.PreConditions[(int)(object)type] = value;
            return this;
        }

        public MethodBuilder<T> SubTasks(params ITaskBuilder[] taskBuilders)
        {
            foreach (var taskBuilder in taskBuilders)
            {
                Method.SubTasks.Add(taskBuilder.Task);
            }

            return this;
        }
    }
}
