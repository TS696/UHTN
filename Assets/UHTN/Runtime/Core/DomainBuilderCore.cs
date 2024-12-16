using System.Collections.Generic;
using Unity.Collections;

namespace UHTN
{
    public class DomainBuilderCore
    {
        private readonly WorldStateDescription _worldStateDesc;
        private readonly Dictionary<ITask, int> _taskIndexDictionary = new();
        private readonly List<ITask> _tasks = new();

        public DomainBuilderCore(WorldStateDescription worldStateDesc)
        {
            _worldStateDesc = worldStateDesc;
        }

        public void AddTask(ITask task)
        {
            _taskIndexDictionary.Add(task, _tasks.Count);
            _tasks.Add(task);
        }

        public bool ContainsTask(ITask task)
        {
            return _taskIndexDictionary.ContainsKey(task);
        }

        private int GetTaskIndex(ITask task)
        {
            return _taskIndexDictionary[task];
        }

        public Domain Resolve()
        {
            var stateLength = _worldStateDesc.StateLength;
            var domain = new Domain(_worldStateDesc, _tasks.ToArray());

            var tasks = new NativeArray<TaskToDecompose>(_tasks.Count, Allocator.Persistent);
            var taskPreconditions = new NativeList<ConditionToDecompose>(_tasks.Count * stateLength, Allocator.Persistent);
            var taskEffects = new NativeList<EffectToDecompose>(_tasks.Count * stateLength, Allocator.Persistent);
            var taskMethodIndices = new NativeArray<ValueRange>(_tasks.Count, Allocator.Persistent);
            var methodSubtasks = new NativeList<SubTaskToDecompose>(10, Allocator.Persistent);
            var methods = new NativeList<MethodToDecompose>(10, Allocator.Persistent);
            var methodPreconditions = new NativeList<ConditionToDecompose>(10, Allocator.Persistent);

            var sumTaskPreconditionCount = 0;
            var sumTaskEffectCount = 0;
            var sumMethodCount = 0;
            var sumMethodPreconditionCount = 0;
            for (var i = 0; i < _tasks.Count; i++)
            {
                var taskType = _tasks[i].Type;

                if (taskType == TaskType.Primitive)
                {
                    var primitiveTask = (IPrimitiveTask)_tasks[i];

                    var preconditionRange = new ValueRange(sumTaskPreconditionCount, primitiveTask.Preconditions.Count);
                    var effectRange = new ValueRange(sumTaskEffectCount, primitiveTask.Effects.Count);

                    foreach (var precondition in primitiveTask.Preconditions)
                    {
                        taskPreconditions.Add(precondition);
                    }

                    sumTaskPreconditionCount += primitiveTask.Preconditions.Count;

                    foreach (var effect in primitiveTask.Effects)
                    {
                        taskEffects.Add(effect);
                    }

                    sumTaskEffectCount += primitiveTask.Effects.Count;

                    var taskToDecompose = new TaskToDecompose(taskType, preconditionRange, effectRange);
                    tasks[i] = taskToDecompose;
                }
                else
                {
                    var compoundTask = (ICompoundTask)_tasks[i];

                    taskMethodIndices[i] = new ValueRange(sumMethodCount, compoundTask.Methods.Count);

                    foreach (var method in compoundTask.Methods)
                    {
                        var methodToDecompose = new MethodToDecompose
                        (
                            new ValueRange(methodSubtasks.Length, method.SubTasks.Count),
                            new ValueRange(sumMethodPreconditionCount, method.Preconditions.Count)
                        );
                        methods.Add(methodToDecompose);
                        foreach (var subtask in method.SubTasks)
                        {
                            var index = GetTaskIndex(subtask.Task);
                            methodSubtasks.Add(new SubTaskToDecompose(index, subtask.DecompositionTiming));
                        }

                        foreach (var condition in method.Preconditions)
                        {
                            methodPreconditions.Add(condition);
                        }

                        sumMethodPreconditionCount += method.Preconditions.Count;
                    }

                    sumMethodCount += compoundTask.Methods.Count;
                    var taskToDecompose = new TaskToDecompose(taskType, default, default);
                    tasks[i] = taskToDecompose;
                }
            }

            domain.Tasks = tasks;
            domain.TaskPreconditions = taskPreconditions;
            domain.TaskEffects = taskEffects;
            domain.TaskMethodIndices = taskMethodIndices;
            domain.MethodSubTasks = methodSubtasks;
            domain.Methods = methods;
            domain.MethodPreconditions = methodPreconditions;

            return domain;
        }
    }
}
