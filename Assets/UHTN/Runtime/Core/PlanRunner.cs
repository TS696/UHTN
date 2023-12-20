using System;
using System.Collections.Generic;
using UHTN.PlanViewer;

namespace UHTN
{
    public class PlanRunner
    {
        public enum RunnerState
        {
            None,
            Running,
            Success,
            Failed
        }


        private Domain _domain;
        public Domain Domain => _domain;

        public IReadOnlyList<Process> Processes => _processList;
        private readonly List<Process> _processList = new();

        public Action OnProcessUpdated;

        public class Process
        {
            public readonly Plan Plan;
            public int OperationIndex;

            public Process(Plan plan)
            {
                Plan = plan;
                OperationIndex = -1;
            }
        }

        public RunnerState State => _state;
        private RunnerState _state;

        private IPrimitiveTask _currentTask;

        public WorldState WorldState => _worldState;
        private WorldState _worldState;

        public int FailedProcessIndex { get; private set; } = -1;

        public string Name { get; set; } = "PlanRunner";

        private bool _isWorldStateDirty;

        private static readonly WorldStatePool worldStatePool = new();

        public void Begin(Domain domain, Plan plan, WorldState worldState)
        {
            _domain = domain;
            _worldState = worldState;

            if (_state == RunnerState.Running)
            {
                StopCurrentOperation();
            }
            else
            {
                OnBegin();
            }

            _processList.Clear();
            FailedProcessIndex = -1;
            _isWorldStateDirty = false;

            _processList.Add(new Process(plan));
            OnProcessUpdated?.Invoke();

            _state = MoveNext();
        }

        public void ReplaceAndResumePlan(int processIndex, Plan newPlan)
        {
            if (_state == RunnerState.Running)
            {
                StopCurrentOperation();
            }
            else
            {
                OnBegin();
            }

            FailedProcessIndex = -1;
            _isWorldStateDirty = false;

            _processList.RemoveRange(processIndex, _processList.Count - processIndex);
            _processList.Add(new Process(newPlan));
            OnProcessUpdated?.Invoke();

            _state = MoveNext();
        }

        private void OnBegin()
        {
            _worldState.OnValueChanged += OnWorldStateChanged;
#if UNITY_EDITOR
            if (PlanViewerRegister.EnablePlanViewer)
            {
                PlanViewerRegister.Register(this);
            }
#endif
        }

        private void OnStop()
        {
            _worldState.OnValueChanged -= OnWorldStateChanged;
#if UNITY_EDITOR
            if (PlanViewerRegister.EnablePlanViewer)
            {
                PlanViewerRegister.UnRegister(this);
            }
#endif
        }

        private void OnWorldStateChanged(WorldState.DirtyReason dirtyReason)
        {
            if (dirtyReason == WorldState.DirtyReason.WorldChanged)
            {
                _isWorldStateDirty = true;
            }
        }

        public void Tick()
        {
            if (_state == RunnerState.Success || _state == RunnerState.Failed)
            {
                return;
            }

            if (_state == RunnerState.None)
            {
                throw new InvalidOperationException("PlanRunner is not running");
            }

            if (_isWorldStateDirty)
            {
                _isWorldStateDirty = false;
                var (isContinue, failedProcessIndex) = CheckCondition(_worldState);
                if (!isContinue)
                {
                    _state = RunnerState.Failed;
                    FailedProcessIndex = failedProcessIndex;
                    StopCurrentOperation();
                    OnStop();
                    return;
                }
            }

            _state = TickOperator();

            if (_state != RunnerState.Running)
            {
                OnStop();
            }
        }

        public void Stop()
        {
            if (_state == RunnerState.None)
            {
                return;
            }

            FailedProcessIndex = -1;
            _state = RunnerState.None;

            StopCurrentOperation();
            OnStop();
        }

        private RunnerState TickOperator()
        {
            if (_currentTask == null)
            {
                var crProcess = _processList[^1];
                var nextTask = (IPrimitiveTask)_domain.GetTask(crProcess.Plan.Tasks[crProcess.OperationIndex]);
                _currentTask = nextTask;
                _currentTask?.OnPreExecute();
                _currentTask?.Operator?.Begin();
            }

            var operatorState = _currentTask?.Operator?.Tick();
            switch (operatorState)
            {
                case OperatorState.Running:
                    return RunnerState.Running;

                case OperatorState.Success:
                case null:
                    StopCurrentOperation();
                    var nextState = MoveNext();
                    OnProcessUpdated?.Invoke();
                    return nextState;

                case OperatorState.Failed:
                    StopCurrentOperation();
                    FailedProcessIndex = _processList.Count - 1;
                    return RunnerState.Failed;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void StopCurrentOperation()
        {
            _currentTask?.Operator?.End();
            _currentTask?.OnPostExecute();
            _currentTask = null;
        }

        private (bool, int) CheckCondition(WorldState worldState)
        {
            var tempWorldState = worldStatePool.Rent(worldState.StateLength);
            worldState.CopyTo(tempWorldState);

            for (var processIndex = _processList.Count - 1; processIndex >= 0; processIndex--)
            {
                var process = _processList[processIndex];

                var startIndex = process.OperationIndex;
                var plan = process.Plan;
                for (var opIdx = startIndex; opIdx < plan.Tasks.Length; opIdx++)
                {
                    var taskIndex = plan.Tasks[opIdx];
                    if (!CheckTaskCondition(taskIndex, tempWorldState))
                    {
                        worldStatePool.Return(tempWorldState);
                        return (false, processIndex);
                    }

                    ApplyTaskEffect(taskIndex, tempWorldState, true);
                }
            }

            worldStatePool.Return(tempWorldState);
            return (true, -1);
        }

        private bool CheckTaskCondition(int taskIndex, WorldState worldState)
        {
            var start = taskIndex * worldState.StateLength;
            for (var i = 0; i < worldState.StateLength; i++)
            {
                var offsetIndex = start + i;
                if (!_domain.TaskPreconditions[offsetIndex].Check(worldState.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyTaskEffect(int taskIndex, WorldState worldState, bool isExpect)
        {
            var start = taskIndex * worldState.StateLength;
            for (var i = 0; i < worldState.StateLength; i++)
            {
                var offsetIndex = start + i;
                var effect = _domain.TaskEffects[offsetIndex];
                if (!isExpect && effect.Type == StateEffectType.PlanOnly)
                {
                    continue;
                }

                worldState.SetValue(i, effect.Apply(worldState.Values[i]), WorldState.DirtyReason.PlanRunner);
            }
        }

        private RunnerState MoveNext()
        {
            var crProcess = _processList[^1];

            if (crProcess.OperationIndex >= 0)
            {
                var taskIndex = crProcess.Plan.Tasks[crProcess.OperationIndex];
                ApplyTaskEffect(taskIndex, _worldState, false);
            }

            if (crProcess.Plan.IsEmpty || crProcess.OperationIndex + 1 >= crProcess.Plan.Tasks.Length)
            {
                _processList.RemoveAt(_processList.Count - 1);
                if (_processList.Count <= 0)
                {
                    return RunnerState.Success;
                }

                return MoveNext();
            }

            crProcess.OperationIndex++;

            return BeginNextTask();
        }

        private RunnerState BeginNextTask()
        {
            var crProcess = _processList[^1];
            var nextTaskIndex = crProcess.Plan.Tasks[crProcess.OperationIndex];
            if (_domain.TaskAttributes[nextTaskIndex].Type == TaskType.Compound)
            {
                if (!PlannerCore.PlanImmediate(_domain, _worldState, out var partialPlan, nextTaskIndex))
                {
                    FailedProcessIndex = _processList.Count - 1;
                    return RunnerState.Failed;
                }

                crProcess.Plan.PartialPlans.Add((crProcess.OperationIndex, partialPlan));
                var nextProcess = new Process(partialPlan);
                _processList.Add(nextProcess);
                if (nextProcess.Plan.Tasks.Length > 0 && nextProcess.Plan.Tasks[0] == nextTaskIndex)
                {
                    throw new InvalidOperationException($"Domain cause infinite loop. taskIndex: {nextTaskIndex}");
                }

                return MoveNext();
            }

            return RunnerState.Running;
        }

        public IEnumerable<(ITask task, int depth, bool isComplete)> IterateSchedule()
        {
            if (_processList.Count <= 0)
            {
                yield break;
            }

            var rootProcess = _processList[0];
            var plan = rootProcess.Plan;
            var operationIndex = rootProcess.OperationIndex;

            foreach (var seg in IterateScheduleSegment(plan, operationIndex, 0))
            {
                yield return seg;
            }
        }

        private IEnumerable<(ITask task, int depth, bool isComplete)> IterateScheduleSegment(Plan plan,
            int operationIndex, int depth)
        {
            for (var i = 0; i < plan.Tasks.Length; i++)
            {
                var task = plan.Tasks[i];
                var isCompleted = i < operationIndex;

                var taskInstance = Domain.GetTask(task);
                if (taskInstance is ICompoundTask compoundTask)
                {
                    var partialPlanIndex = plan.PartialPlans.FindIndex(x => x.Item1 == i);
                    if (partialPlanIndex < 0)
                    {
                        continue;
                    }

                    var partialPlan = plan.PartialPlans[partialPlanIndex];
                    var partialPlanOperationIndex = 0;

                    if (i == operationIndex)
                    {
                        partialPlanOperationIndex = _processList.Find(p => p.Plan == partialPlan.plan).OperationIndex;
                    }
                    else
                    {
                        partialPlanOperationIndex = partialPlan.plan.Tasks.Length;
                    }

                    yield return (compoundTask, depth, isCompleted);

                    foreach (var seg in IterateScheduleSegment(partialPlan.plan, partialPlanOperationIndex, depth + 1))
                    {
                        yield return seg;
                    }

                    yield break;
                }

                var primitiveTask = (IPrimitiveTask)Domain.GetTask(task);
                yield return (primitiveTask, depth, isCompleted);
            }
        }
    }
}
