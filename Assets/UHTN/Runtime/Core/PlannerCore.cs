using System;
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
            private NativeReference<bool> _result;

            internal PlanHandle(JobHandle handle, NativeList<int> decomposedTasks,
                int targetTaskIndex, NativeList<int> methodTraversalRecord, NativeReference<bool> result)
            {
                _handle = handle;
                _decomposedTasks = decomposedTasks;
                _targetTaskIndex = targetTaskIndex;
                _methodTraversalRecord = methodTraversalRecord;
                _result = result;
            }

            public bool Complete(out Plan plan)
            {
                _handle.Complete();
                var resultTasks = _decomposedTasks.AsArray().ToArray();
                var resultMtr = _methodTraversalRecord.AsArray().ToArray();
                var isSuccess = _result.Value;

                _methodTraversalRecord.Dispose();
                _decomposedTasks.Dispose();
                _result.Dispose();

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
            var result = new NativeReference<bool>(Allocator.TempJob);

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
                Result = result
            };

            var jobHandle = job.Schedule();
            return new PlanHandle(jobHandle, decomposedTasks, targetTaskIndex, methodTraversalRecord, result);
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
            public NativeReference<bool> Result;

            private struct DecompositionContext : IDisposable
            {
                public NativeList<int> TaskProcessStack;
                public NativeList<int> WorldStateStack;
                public NativeArray<int> WorkingWorldState;
                public NativeList<MethodDecomposition> MethodStack;

                public DecompositionContext(NativeArray<int> inputWorldState)
                {
                    TaskProcessStack = new NativeList<int>(Allocator.Temp);
                    WorldStateStack = new NativeList<int>(Allocator.Temp);
                    WorkingWorldState = new NativeArray<int>(inputWorldState, Allocator.Temp);
                    MethodStack = new NativeList<MethodDecomposition>(Allocator.Temp);
                }

                public void Dispose()
                {
                    TaskProcessStack.Dispose();
                    WorldStateStack.Dispose();
                    WorkingWorldState.Dispose();
                    MethodStack.Dispose();
                }
            }

            private readonly struct MethodDecomposition
            {
                public readonly int TaskIndex;
                public readonly int MethodIndexOffset;
                public readonly int ProcessStackCount;
                public readonly int DecomposedTaskIndex;

                public MethodDecomposition(int taskIndex, int methodIndexOffset, int processStackCount,
                    int decomposedTaskIndex)
                {
                    TaskIndex = taskIndex;
                    MethodIndexOffset = methodIndexOffset;
                    ProcessStackCount = processStackCount;
                    DecomposedTaskIndex = decomposedTaskIndex;
                }
            }

            public void Execute()
            {
                using var context = new DecompositionContext(InputWorldState);

                var isSuccess = DecomposeTask(context);
                if (!isSuccess)
                {
                    DecomposedTasks.Clear();
                }

                Result.Value = isSuccess;
            }

            private bool DecomposeTask(DecompositionContext context)
            {
                var nextMethodIndexOffset = 0;
                context.TaskProcessStack.Add(TargetTaskIndex);

                while (context.TaskProcessStack.Length > 0)
                {
                    var currentTaskIndex = context.TaskProcessStack[^1];
                    context.TaskProcessStack.RemoveAt(context.TaskProcessStack.Length - 1);

                    // Decompose primitive task
                    if (TaskAttributes[currentTaskIndex].Type == TaskType.Primitive)
                    {
                        if (!IsValidCondition(ref context.WorkingWorldState, currentTaskIndex, ref TaskPreconditions))
                        {
                            if (!PopMethod(ref context, out nextMethodIndexOffset))
                            {
                                return false;
                            }

                            continue;
                        }

                        ApplyTask(currentTaskIndex, ref context.WorkingWorldState);
                        DecomposedTasks.Add(currentTaskIndex);
                    }
                    // Decompose compound task
                    else if (TaskAttributes[currentTaskIndex].Type == TaskType.Compound)
                    {
                        var isRootTask = context.MethodStack.Length <= 0;
                        if (!isRootTask && TaskAttributes[currentTaskIndex].DecompositionTiming == DecompositionTiming.Delayed)
                        {
                            DecomposedTasks.Add(currentTaskIndex);
                            continue;
                        }

                        var range = TaskMethodIndices[currentTaskIndex];
                        var moveNext = false;
                        for (var methodIndex = range.Start + nextMethodIndexOffset; methodIndex < range.End; methodIndex++)
                        {
                            if (IsValidCondition(ref context.WorkingWorldState, methodIndex, ref MethodPreconditions))
                            {
                                // Decompose Method
                                PushMethod(currentTaskIndex, methodIndex - range.Start, ref context);

                                var subTaskRange = MethodSubTaskIndices[methodIndex];
                                for (var i = subTaskRange.End - 1; i >= subTaskRange.Start; i--)
                                {
                                    var subTask = MethodSubTasks[i];
                                    context.TaskProcessStack.Add(subTask.TaskIndex);
                                }

                                moveNext = true;
                                break;
                            }
                        }

                        if (!moveNext && !PopMethod(ref context, out nextMethodIndexOffset))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            private void PushMethod(int taskIndex, int methodIndexOffset, ref DecompositionContext context)
            {
                MethodTraversalRecord.Add(methodIndexOffset);
                context.MethodStack.Add(new MethodDecomposition(taskIndex, methodIndexOffset, context.TaskProcessStack.Length,
                    DecomposedTasks.Length));

                context.WorldStateStack.AddRange(context.WorkingWorldState);
            }

            private bool PopMethod(ref DecompositionContext context, out int nextMethodIndex)
            {
                nextMethodIndex = -1;
                if (context.MethodStack.Length <= 0)
                {
                    return false;
                }

                var lastMethod = context.MethodStack[^1];
                context.MethodStack.RemoveAt(context.MethodStack.Length - 1);
                DecomposedTasks.RemoveRange(lastMethod.DecomposedTaskIndex, DecomposedTasks.Length - lastMethod.DecomposedTaskIndex);
                context.TaskProcessStack.RemoveRange(lastMethod.ProcessStackCount, context.TaskProcessStack.Length - lastMethod.ProcessStackCount);

                var hasNextMethod = lastMethod.MethodIndexOffset + 1 < TaskMethodIndices[lastMethod.TaskIndex].Length;
                if (hasNextMethod)
                {
                    // continue from the next method
                    context.TaskProcessStack.Add(lastMethod.TaskIndex);
                }
                else if (context.MethodStack.Length <= 0)
                {
                    // root task has no more methods
                    return false;
                }

                MethodTraversalRecord.RemoveAt(MethodTraversalRecord.Length - 1);
                nextMethodIndex = lastMethod.MethodIndexOffset + 1;

                // pop world state
                var startIdx = context.WorldStateStack.Length - StateLength;
                for (var i = 0; i < StateLength; i++)
                {
                    context.WorkingWorldState[i] = context.WorldStateStack[startIdx + i];
                }

                context.WorldStateStack.ResizeUninitialized(context.WorldStateStack.Length - StateLength);
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
