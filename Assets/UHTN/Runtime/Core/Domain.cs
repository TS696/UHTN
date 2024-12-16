using System;
using Unity.Collections;

namespace UHTN
{
    public class Domain : IDisposable
    {
        private readonly WorldStateDescription _worldStateDescription;

        public NativeArray<TaskToDecompose> Tasks;

        public NativeList<ConditionToDecompose> TaskPreconditions;

        public NativeList<EffectToDecompose> TaskEffects;

        public NativeArray<ValueRange> TaskMethodIndices;

        public NativeList<SubTaskToDecompose> MethodSubTasks;

        public NativeList<MethodToDecompose> Methods;

        public NativeList<ConditionToDecompose> MethodPreconditions;

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
            Tasks.Dispose();
            TaskPreconditions.Dispose();
            TaskEffects.Dispose();
            TaskMethodIndices.Dispose();
            MethodSubTasks.Dispose();
            Methods.Dispose();
            MethodPreconditions.Dispose();
        }
    }
}
