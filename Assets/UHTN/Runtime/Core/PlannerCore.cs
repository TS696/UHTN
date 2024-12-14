using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UHTN
{
    public static class PlannerCore
    {
        public struct PlanHandle
        {
            private JobHandle _handle;
            private NativeList<int> _decomposedTasks;
            private readonly int _targetTaskIndex;
            private NativeList<int> _methodTraversalRecord;
            private NativeArray<bool> _results;

            internal PlanHandle(JobHandle handle, NativeList<int> decomposedTasks,
                int targetTaskIndex, NativeList<int> methodTraversalRecord, NativeArray<bool> results)
            {
                _handle = handle;
                _decomposedTasks = decomposedTasks;
                _targetTaskIndex = targetTaskIndex;
                _methodTraversalRecord = methodTraversalRecord;
                _results = results;
            }

            public bool Complete(out Plan plan)
            {
                _handle.Complete();
                var resultTasks = _decomposedTasks.AsArray().ToArray();
                var resultMtr = _methodTraversalRecord.AsArray().ToArray();
                var isSuccess = _results[0];

                _methodTraversalRecord.Dispose();
                _decomposedTasks.Dispose();
                _results.Dispose();

                if (isSuccess)
                {
                    plan = new Plan(resultTasks, _targetTaskIndex, resultMtr);
                    return true;
                }

                plan = null;
                return false;
            }
        }

        public static bool PlanImmediate(Domain domain, WorldState worldState, out Plan plan, int targetTaskIndex = 0)
        {
            var planHandle = Plan(domain, worldState, targetTaskIndex);
            return planHandle.Complete(out plan);
        }

        public static PlanHandle Plan(Domain domain, WorldState worldState, int targetTaskIndex = 0)
        {
            var nativeWorldState = worldState.ToNativeArray(Allocator.TempJob);

            var decomposedTasks = new NativeList<int>(10, Allocator.TempJob);
            var methodTraversalRecord = new NativeList<int>(10, Allocator.TempJob);
            var results = new NativeArray<bool>(1, Allocator.TempJob);

            var job = new PlanJob()
            {
                InputWorldState = nativeWorldState,
                TargetTaskIndex = targetTaskIndex,
                TaskAttributes = domain.TaskAttributes,
                TaskPreconditions = domain.TaskPreconditions,
                TaskEffects = domain.TaskEffects,
                TaskMethodIndices = domain.TaskMethodIndices,
                MethodSubTasks = domain.MethodSubTasks.AsArray(),
                MethodSubTaskIndices = domain.MethodSubTaskIndices.AsArray(),
                MethodPreconditions = domain.MethodPreconditions.AsArray(),
                DecomposedTasks = decomposedTasks,
                MethodTraversalRecord = methodTraversalRecord,
                Results = results
            };

            var jobHandle = job.Schedule();
            return new PlanHandle(jobHandle, decomposedTasks, targetTaskIndex, methodTraversalRecord, results);
        }

        [BurstCompile]
        private struct PlanJob : IJob
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<int> InputWorldState;

            private int StateLength => InputWorldState.Length;

            public int TargetTaskIndex;

            [ReadOnly]
            public NativeArray<TaskAttribute> TaskAttributes;

            [ReadOnly]
            public NativeArray<StateCondition> TaskPreconditions;

            [ReadOnly]
            public NativeArray<StateEffect> TaskEffects;

            [ReadOnly]
            public NativeArray<ValueRange> TaskMethodIndices;

            [ReadOnly]
            public NativeArray<SubTask> MethodSubTasks;

            [ReadOnly]
            public NativeArray<ValueRange> MethodSubTaskIndices;

            [ReadOnly]
            public NativeArray<StateCondition> MethodPreconditions;
            
            public NativeList<int> DecomposedTasks;

            public NativeList<int> MethodTraversalRecord;

            [WriteOnly]
            public NativeArray<bool> Results;
           
            private readonly struct MethodDecomposition
            {
                public readonly int TaskIndex;
                public readonly bool IsRootTask;
                public readonly int MethodIndex;
                public readonly int ProcessStackCount;
                public readonly int DecomposedTaskIndex;

                public MethodDecomposition(int taskIndex, bool isRootTask, int methodIndex, int processStackCount, int decomposedTaskIndex)
                {
                    TaskIndex = taskIndex;
                    IsRootTask = isRootTask;
                    MethodIndex = methodIndex;
                    ProcessStackCount = processStackCount;
                    DecomposedTaskIndex = decomposedTaskIndex;
                }
            }

            public void Execute()
            {
                var worldState = CloneWorldState(ref InputWorldState);
                var isSuccess = DecomposeTask(TargetTaskIndex, ref DecomposedTasks, ref MethodTraversalRecord,
                    ref worldState);
                if (!isSuccess)
                {
                    DecomposedTasks.Clear();
                }

                Results[0] = isSuccess;
            }

            private bool DecomposeTask(int taskIndex, ref NativeList<int> decomposedTasks,
                ref NativeList<int> methodTraversalRecord, ref NativeArray<int> worldState)
            {
                var processStack = new NativeList<int>(Allocator.Temp);
                var worldStateStack = new NativeList<int>(Allocator.Temp);
                var workingWorldState = CloneWorldState(ref worldState);
                var methodStack = new NativeList<MethodDecomposition>(Allocator.Temp);
                var nextMethodIndex = 0;
                processStack.Add(taskIndex);
                var isRootTask = true;

                while (processStack.Length > 0)
                {
                    var currentTask = processStack[^1];
                    processStack.RemoveAt(processStack.Length - 1);

                    // Decompose primitive task
                    if (TaskAttributes[currentTask].Type == TaskType.Primitive)
                    {
                        isRootTask = false;
                        if (!IsValidCondition(ref workingWorldState, currentTask, ref TaskPreconditions))
                        {
                            if (!PopMethod(true, ref methodStack, ref processStack, ref methodTraversalRecord, ref decomposedTasks,
                                    out nextMethodIndex, out isRootTask))
                            {
                                return false;
                            }
                            PopWorldState(ref worldStateStack, ref workingWorldState);
                            continue;
                        }

                        ApplyTask(currentTask, ref workingWorldState);
                        decomposedTasks.Add(currentTask);
                    }
                    // Decompose compound task
                    else if (TaskAttributes[currentTask].Type == TaskType.Compound)
                    {
                        if (!isRootTask && TaskAttributes[currentTask].DecompositionTiming == DecompositionTiming.Delayed)
                        {
                            decomposedTasks.Add(currentTask);
                            continue;
                        }

                        var range = TaskMethodIndices[currentTask];
                        var moveNext = false;
                        for (var methodIndex = range.Start + nextMethodIndex; methodIndex < range.End; methodIndex++)
                        {
                            if (IsValidCondition(ref workingWorldState, methodIndex, ref MethodPreconditions))
                            {
                                // Decompose Method
                                methodTraversalRecord.Add(methodIndex);
                                methodStack.Add(new MethodDecomposition(currentTask, isRootTask, methodIndex - range.Start, processStack.Length, decomposedTasks.Length));
                                PushWorldState(ref worldStateStack, ref workingWorldState);

                                var subTaskRange = MethodSubTaskIndices[methodIndex];
                                for (var i = subTaskRange.End - 1; i >= subTaskRange.Start; i--)
                                {
                                    var subTask = MethodSubTasks[i];
                                    processStack.Add(subTask.TaskIndex);
                                }

                                moveNext = true;
                                break;
                            }
                        }

                        isRootTask = false;

                        if (!moveNext)
                        {
                            if (!PopMethod(false, ref methodStack, ref processStack, ref methodTraversalRecord, ref decomposedTasks,
                                    out nextMethodIndex, out isRootTask))
                            {
                                return false;
                            }
                        }
                    }
                }


                processStack.Dispose();
                worldStateStack.Dispose();
                workingWorldState.Dispose();
                methodStack.Dispose();
                return true;
            }

            private NativeArray<int> CloneWorldState(ref NativeArray<int> worldState)
            {
                var clone = new NativeArray<int>(worldState.Length, Allocator.Temp);
                worldState.CopyTo(clone);
                return clone;
            }

            private void PushWorldState(ref NativeList<int> worldStateStack, ref NativeArray<int> worldState)
            {
                worldStateStack.AddRange(worldState);
            }

            private void PopWorldState(ref NativeList<int> worldStateStack, ref NativeArray<int> outWorldState)
            {
                var startIdx = worldStateStack.Length - StateLength;
                for (var i = 0; i < StateLength; i++)
                {
                    outWorldState[i] = worldStateStack[startIdx + i];
                }

                worldStateStack.ResizeUninitialized(worldStateStack.Length - StateLength);
            }

            private bool PopMethod(bool continueTask, ref NativeList<MethodDecomposition> methodStack, ref NativeList<int> processStack, ref NativeList<int> methodTraversalRecord, ref NativeList<int> decomposedTasks, out int nextMethodIndex, out bool isRootTask)
            {
                if (methodStack.Length <= 0)
                {
                    nextMethodIndex = -1;
                    isRootTask = false;
                    return false;
                }

                var lastMethod = methodStack[^1];
                methodStack.RemoveAt(methodStack.Length - 1);
                decomposedTasks.RemoveRange(lastMethod.DecomposedTaskIndex, decomposedTasks.Length - lastMethod.DecomposedTaskIndex);
                processStack.RemoveRange(lastMethod.ProcessStackCount, processStack.Length - lastMethod.ProcessStackCount);
                if (continueTask)
                {
                    // continue from the next method
                    processStack.Add(lastMethod.TaskIndex);
                    isRootTask = lastMethod.IsRootTask;
                }
                else
                {
                    isRootTask = false;
                }

                methodTraversalRecord.RemoveAt(methodTraversalRecord.Length - 1);

                nextMethodIndex = lastMethod.MethodIndex + 1;
                return true;
            }

            private void ApplyTask(int taskIndex, ref NativeArray<int> worldState)
            {
                var start = taskIndex * StateLength;
                for (var i = 0; i < StateLength; i++)
                {
                    var offsetIndex = start + i;
                    worldState[i] = TaskEffects[offsetIndex].Apply(worldState[i]);
                }
            }

            private bool IsValidCondition(ref NativeArray<int> worldState, int preconditionIndex,
                ref NativeArray<StateCondition> preconditions)
            {
                var start = preconditionIndex * StateLength;

                for (var i = 0; i < StateLength; i++)
                {
                    var offsetIndex = start + i;
                    if (!preconditions[offsetIndex].Check(worldState[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
