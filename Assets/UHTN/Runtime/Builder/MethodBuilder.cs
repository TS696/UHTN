using System;

namespace UHTN.Builder
{
    public class MethodBuilder<T> where T : Enum
    {
        public Method Method => _method;

        private readonly TaskBuildHelper<T> _helper;
        private readonly Method _method;

        public MethodBuilder(TaskBuildHelper<T> helper, Method method)
        {
            _helper = helper;
            _method = method;
        }

        public MethodBuilder<T> Precondition(T type, StateCondition value)
        {
            _helper.SetCondition(_method.PreConditions, type, value);
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
