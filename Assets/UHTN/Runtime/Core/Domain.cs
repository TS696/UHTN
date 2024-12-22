using System;
using Unity.Collections;

namespace UHTN
{
    public class Domain : IDisposable
    {
        private readonly WorldStateDescription _worldStateDesc;

        public NativeArray<TaskToDecompose> Tasks { get; private set; }

        public NativeList<ConditionToDecompose> TaskPreconditions { get; private set; }

        public NativeList<EffectToDecompose> TaskEffects { get; private set; }

        public NativeArray<ValueRange> TaskMethodIndices { get; private set; }

        public NativeList<SubTaskToDecompose> MethodSubTasks { get; private set; }

        public NativeList<MethodToDecompose> Methods { get; private set; }

        public NativeList<ConditionToDecompose> MethodPreconditions { get; private set; }

        private readonly ITask[] _tasks;

        internal Domain(WorldStateDescription worldStateDesc, ITask[] tasks)
        {
            _worldStateDesc = worldStateDesc;
            _tasks = tasks;
        }

        internal void Initialize(NativeArray<TaskToDecompose> tasks, NativeList<ConditionToDecompose> taskPreconditions,
            NativeList<EffectToDecompose> taskEffects, NativeArray<ValueRange> taskMethodIndices,
            NativeList<SubTaskToDecompose> methodSubTasks, NativeList<MethodToDecompose> methods,
            NativeList<ConditionToDecompose> methodPreconditions)
        {
            Tasks = tasks;
            TaskPreconditions = taskPreconditions;
            TaskEffects = taskEffects;
            TaskMethodIndices = taskMethodIndices;
            MethodSubTasks = methodSubTasks;
            Methods = methods;
            MethodPreconditions = methodPreconditions;
        }

        public ITask GetTask(int taskIndex)
        {
            return _tasks[taskIndex];
        }

        public WorldState CreateWorldState()
        {
            return _worldStateDesc.CreateWorldState();
        }
        
        public int StateLength => _worldStateDesc.StateLength;

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
