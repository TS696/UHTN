using System;
using Unity.Collections;

namespace UHTN
{
    public class Domain : IDisposable
    {
        public readonly WorldStateDescription WorldStateDesc;
        public int StateLength => WorldStateDesc.StateLength;

        public NativeArray<TaskAttribute> TaskAttributes;

        public NativeArray<StateCondition> TaskPreconditions;

        public NativeArray<StateEffect> TaskEffects;

        public NativeArray<ValueRange> TaskMethodIndices;

        public NativeList<SubTask> MethodSubTasks;

        public NativeList<ValueRange> MethodSubTaskIndices;

        public NativeList<StateCondition> MethodPreconditions;

        private readonly ITask[] _tasks;

        internal Domain(WorldStateDescription worldStateDesc, ITask[] tasks)
        {
            WorldStateDesc = worldStateDesc;
            _tasks = tasks;
        }

        public ITask GetTask(int taskIndex)
        {
            return _tasks[taskIndex];
        }

        public void Dispose()
        {
            TaskAttributes.Dispose();
            TaskPreconditions.Dispose();
            TaskEffects.Dispose();
            TaskMethodIndices.Dispose();
            MethodSubTasks.Dispose();
            MethodSubTaskIndices.Dispose();
            MethodPreconditions.Dispose();
        }
    }
}
