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

        private int GetTaskIndex(ITask task)
        {
            return _taskIndexDictionary[task];
        }

        public Domain Resolve()
        {
            var stateLength = _worldStateDesc.StateLength;
            var domain = new Domain(_worldStateDesc, _tasks.ToArray());

            var taskAttributes = new NativeArray<TaskAttribute>(_tasks.Count, Allocator.Persistent);
            var taskPreconditions = new NativeArray<StateCondition>(_tasks.Count * stateLength, Allocator.Persistent);
            var taskEffects = new NativeArray<StateEffect>(_tasks.Count * stateLength, Allocator.Persistent);
            var taskMethodIndices = new NativeArray<ValueRange>(_tasks.Count, Allocator.Persistent);
            var methodSubTasks = new NativeList<SubTask>(10, Allocator.Persistent);
            var methodSubTaskIndices = new NativeList<ValueRange>(10, Allocator.Persistent);
            var methodPreConditions = new NativeList<StateCondition>(10, Allocator.Persistent);

            var sumMethodCount = 0;
            for (var i = 0; i < _tasks.Count; i++)
            {
                taskAttributes[i] = _tasks[i].Attribute;

                if (taskAttributes[i].Type == TaskType.Primitive)
                {
                    var primitiveTask = (IPrimitiveTask)_tasks[i];
                    NativeArray<StateCondition>.Copy(primitiveTask.PreConditions, 0, taskPreconditions,
                        i * stateLength, stateLength);
                    NativeArray<StateEffect>.Copy(primitiveTask.Effects, 0, taskEffects,
                        i * stateLength, stateLength);
                }
                else
                {
                    var compoundTask = (ICompoundTask)_tasks[i];

                    taskMethodIndices[i] = new ValueRange(sumMethodCount, compoundTask.Methods.Count);

                    foreach (var method in compoundTask.Methods)
                    {
                        methodSubTaskIndices.Add(new ValueRange(methodSubTasks.Length, method.SubTasks.Count));
                        foreach (var subtask in method.SubTasks)
                        {
                            var index = GetTaskIndex(subtask);
                            methodSubTasks.Add(new SubTask(index));
                        }

                        foreach (var condition in method.PreConditions)
                        {
                            methodPreConditions.Add(condition);
                        }
                    }

                    sumMethodCount += compoundTask.Methods.Count;
                }
            }

            domain.TaskAttributes = taskAttributes;
            domain.TaskPreconditions = taskPreconditions;
            domain.TaskEffects = taskEffects;
            domain.TaskMethodIndices = taskMethodIndices;
            domain.MethodSubTasks = methodSubTasks;
            domain.MethodSubTaskIndices = methodSubTaskIndices;
            domain.MethodPreconditions = methodPreConditions;

            return domain;
        }
    }
}
