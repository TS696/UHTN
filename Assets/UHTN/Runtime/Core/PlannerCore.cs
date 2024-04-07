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

            [WriteOnly]
            public NativeList<int> DecomposedTasks;

            [WriteOnly]
            public NativeList<int> MethodTraversalRecord;

            [WriteOnly]
            public NativeArray<bool> Results;

            private const int MaxLoopCount = 1000;
            private int _loopNum;

            public void Execute()
            {
                var worldState = CloneWorldState(InputWorldState);
                var isSuccess = DecomposeTask(TargetTaskIndex, DecomposedTasks, MethodTraversalRecord, worldState,
                    true);
                if (!isSuccess)
                {
                    DecomposedTasks.Clear();
                }

                Results[0] = isSuccess;
            }

            private bool DecomposeTask(int taskIndex, NativeList<int> decomposedTasks,
                NativeList<int> methodTraversalRecord, NativeArray<int> worldState, bool isRoot = false)
            {
                if (MaxLoopCount < _loopNum++)
                {
                    throw new Exception("Max loop count exceeded.");
                }

                return TaskAttributes[taskIndex].Type switch
                {
                    TaskType.Primitive => DecomposePrimitive(taskIndex, decomposedTasks, worldState),
                    TaskType.Compound => DecomposeCompound(taskIndex, decomposedTasks, methodTraversalRecord,
                        ref worldState, isRoot),
                    _ => false
                };
            }


            private bool DecomposePrimitive(int taskIndex, NativeList<int> decomposedTasks, NativeArray<int> worldState)
            {
                if (!IsValidCondition(worldState, taskIndex, TaskPreconditions))
                {
                    return false;
                }

                decomposedTasks.Add(taskIndex);
                return true;
            }

            private bool DecomposeCompound(int taskIndex, NativeList<int> decomposedTasks,
                NativeList<int> methodTraversalRecord, ref NativeArray<int> worldState, bool isRoot)
            {
                if (!isRoot && TaskAttributes[taskIndex].DecompositionTiming == DecompositionTiming.Delayed)
                {
                    decomposedTasks.Add(taskIndex);
                    return true;
                }

                var range = TaskMethodIndices[taskIndex];
                for (var methodIndex = range.Start; methodIndex < range.End; methodIndex++)
                {
                    if (IsValidCondition(worldState, methodIndex, MethodPreconditions))
                    {
                        var cloneState = CloneWorldState(worldState);
                        var subMethodTraversalRecord = new NativeList<int>(Allocator.Temp);
                        var subDecomposedTasks = new NativeList<int>(Allocator.Temp);
                        if (DecomposeMethod(methodIndex, subDecomposedTasks, subMethodTraversalRecord, worldState))
                        {
                            methodTraversalRecord.Add(methodIndex);
                            methodTraversalRecord.AddRange(subMethodTraversalRecord);
                            decomposedTasks.AddRange(subDecomposedTasks);

                            cloneState.Dispose();
                            subMethodTraversalRecord.Dispose();
                            subDecomposedTasks.Dispose();
                            return true;
                        }

                        worldState.Dispose();
                        worldState = cloneState;
                        subMethodTraversalRecord.Dispose();
                        subDecomposedTasks.Dispose();
                    }
                }

                return false;
            }

            private bool DecomposeMethod(int methodIndex, NativeList<int> decomposedTasks,
                NativeList<int> methodTraversalRecord, NativeArray<int> worldState)
            {
                var success = true;
                var range = MethodSubTaskIndices[methodIndex];

                for (var i = range.Start; i < range.End; i++)
                {
                    var subTask = MethodSubTasks[i];
                    if (!DecomposeTask(subTask.TaskIndex, decomposedTasks, methodTraversalRecord, worldState))
                    {
                        success = false;
                        break;
                    }

                    ApplyTask(subTask.TaskIndex, worldState);
                }

                return success;
            }

            private NativeArray<int> CloneWorldState(NativeArray<int> worldState)
            {
                var clone = new NativeArray<int>(worldState.Length, Allocator.Temp);
                worldState.CopyTo(clone);
                return clone;
            }

            private void ApplyTask(int taskIndex, NativeArray<int> worldState)
            {
                var start = taskIndex * StateLength;
                for (var i = 0; i < StateLength; i++)
                {
                    var offsetIndex = start + i;
                    worldState[i] = TaskEffects[offsetIndex].Apply(worldState[i]);
                }
            }

            private bool IsValidCondition(NativeArray<int> worldState, int preconditionIndex,
                NativeArray<StateCondition> preconditions)
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
