using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UHTN
{
    public enum PlannerExecutionType
    {
        RunUntilSuccess,
        RePlanForever
    }

    public class Planner : IDisposable
    {
        public bool IsRunning { get; private set; }

        public PlannerExecutionType ExecutionType { get; set; }

        private readonly PlanRunner _planRunner = new();
        public PlanRunner PlanRunner => _planRunner;

        private readonly Domain _domain;

        private readonly WorldState _worldState;

        private bool _isWorldStateDirty;

        private readonly PlanRequest _planRequest = new();

        public Planner(Domain domain, WorldState worldState)
        {
            _domain = domain;
            _worldState = worldState;

            _worldState.OnValueChanged += SetWorldStateDirty;
        }

        private enum PlanRequestType
        {
            Begin,
            ReplaceAndResume
        }

        private class PlanRequest : IDisposable
        {
            public bool HasRequest { get; private set; }
            public PlannerCore.PlanHandle Handle { get; private set; }
            public PlanRequestType RequestType { get; private set; }
            public int ProcessIndex { get; private set; }

            public void Request(PlanRequestType requestType, PlannerCore.PlanHandle handle, int processIndex = 0)
            {
                if (HasRequest)
                {
                    Complete(out _);
                }

                HasRequest = true;
                RequestType = requestType;
                Handle = handle;
                ProcessIndex = processIndex;
            }

            public bool Complete(out Plan plan)
            {
                if (!HasRequest)
                {
                    throw new InvalidOperationException("No request");
                }

                HasRequest = false;
                return Handle.Complete(out plan);
            }

            public void Dispose()
            {
                if (HasRequest)
                {
                    Complete(out _);
                }
            }
        }

        public void Begin()
        {
            _isWorldStateDirty = false;
            RequestPlan(PlanRequestType.Begin);
            IsRunning = true;
        }

        public void Stop()
        {
            Pause();
            _planRunner.Stop();
            IsRunning = false;
        }

        public void Pause()
        {
            if (_planRequest.HasRequest)
            {
                CompletePlanRequest();
            }
        }

        private void SetWorldStateDirty(WorldState.DirtyReason dirtyReason)
        {
            if (!IsRunning)
            {
                return;
            }

            if (dirtyReason == WorldState.DirtyReason.WorldChanged)
            {
                _isWorldStateDirty = true;
            }
        }

        public bool Tick()
        {
            if (_planRequest.HasRequest)
            {
                CompletePlanRequest();
            }

            if (!IsRunning)
            {
                return false;
            }

            if (_isWorldStateDirty)
            {
                OnWorldStateChanged();
                _isWorldStateDirty = false;
            }

            switch (_planRunner.State)
            {
                case PlanRunner.RunnerState.Success:
                    if (ExecutionType == PlannerExecutionType.RunUntilSuccess)
                    {
                        IsRunning = false;
                        return false;
                    }

                    OnSuccess();
                    break;
                case PlanRunner.RunnerState.Failed:
                    OnFailed();
                    break;
            }


            if (_planRunner.State == PlanRunner.RunnerState.Running)
            {
                _planRunner.Tick();
            }

            return true;
        }

        private void RequestPlan(PlanRequestType requestType, int targetTaskIndex = 0, int processIndex = 0)
        {
            _planRequest.Request(requestType, PlannerCore.Plan(_domain, _worldState, targetTaskIndex), processIndex);
        }

        private void CompletePlanRequest()
        {
            if (_planRequest.Complete(out var plan))
            {
                switch (_planRequest.RequestType)
                {
                    case PlanRequestType.Begin:
                        _planRunner.Begin(_domain, plan, _worldState);
                        break;
                    case PlanRequestType.ReplaceAndResume:
                        _planRunner.ReplaceAndResumePlan(_planRequest.ProcessIndex, plan);
                        break;
                }

                return;
            }

            _planRunner.Stop();
        }

        private void OnSuccess()
        {
            RequestPlan(PlanRequestType.Begin);
        }

        private void OnFailed()
        {
            var index = _planRunner.FailedProcessIndex;
            var process = _planRunner.Processes[index];
            var plan = process.Plan;

            RequestPlan(PlanRequestType.ReplaceAndResume, plan.TargetTaskIndex, index);
        }

        private void OnWorldStateChanged()
        {
            switch (_planRunner.State)
            {
                case PlanRunner.RunnerState.Running:
                    CompareAndRePlan();
                    break;

                default:
                    _planRequest.Request(PlanRequestType.Begin, PlannerCore.Plan(_domain, _worldState));
                    break;
            }
        }

        private void CompareAndRePlan()
        {
            var currentProcesses = _planRunner.Processes;
            for (var i = 0; i < currentProcesses.Count; i++)
            {
                var process = currentProcesses[i];
                var plan = process.Plan;
                if (!PlannerCore.PlanImmediate(_domain, _worldState, out var newPlan, plan.TargetTaskIndex))
                {
                    return;
                }

                switch (CompareMtr(plan.MethodTraversalRecord, newPlan.MethodTraversalRecord))
                {
                    case MtrCompareResult.Same:
                        break;

                    case MtrCompareResult.NewPlanIsBetter:
                        _planRunner.ReplaceAndResumePlan(i, newPlan);
                        return;

                    case MtrCompareResult.OldPlanIsBetter:
                        return;
                }
            }
        }

        private enum MtrCompareResult
        {
            Same,
            OldPlanIsBetter,
            NewPlanIsBetter
        }

        private MtrCompareResult CompareMtr(IReadOnlyList<int> oldMtr, IReadOnlyList<int> newMtr)
        {
            if (oldMtr.SequenceEqual(newMtr))
            {
                return MtrCompareResult.Same;
            }

            var length = Mathf.Min(oldMtr.Count, newMtr.Count);
            for (var i = 0; i < length; i++)
            {
                if (newMtr[i] < oldMtr[i])
                {
                    return MtrCompareResult.NewPlanIsBetter;
                }
            }

            return MtrCompareResult.OldPlanIsBetter;
        }

        public void Dispose()
        {
            _planRequest.Dispose();
            _planRunner.Stop();
            _domain.Dispose();
        }
    }
}
