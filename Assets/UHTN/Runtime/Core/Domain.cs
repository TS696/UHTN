using System;
using Unity.Collections;

namespace UHTN
{
    public class Domain : IDisposable
    {
        private readonly WorldStateDescription _worldStateDescription;

        public NativeArray<TaskAttribute> TaskAttributes;

        public NativeArray<StateCondition> TaskPreconditions;

        public NativeArray<StateEffect> TaskEffects;

        public NativeArray<ValueRange> TaskMethodIndices;

        public NativeArray<SubTask> MethodSubTasks;

        public NativeArray<ValueRange> MethodSubTaskIndices;

        public NativeArray<StateCondition> MethodPreconditions;

        private readonly ITask[] _tasks;

        internal Domain(WorldStateDescription worldStateDescription, ITask[] tasks)
        {
            _worldStateDescription = worldStateDescription;
            _tasks = tasks;
        }

        public ITask GetTask(int taskIndex)
        {
            return _tasks[taskIndex];
        }

        public WorldState CreateWorldState()
        {
            return _worldStateDescription.CreateWorldState();
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
