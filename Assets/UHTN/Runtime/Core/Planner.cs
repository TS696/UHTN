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

        public PlanRunner PlanRunner { get; } = new();

        public Domain Domain { get; }

        public WorldState WorldState { get; }

        private bool _isWorldStateDirty;

        private readonly PlanRequest _planRequest = new();

        public Planner(Domain domain, WorldState worldState)
        {
            Domain = domain;
            WorldState = worldState;

            WorldState.OnValueChanged += SetWorldStateDirty;
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
            PlanRunner.Stop();
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
            if (!IsRunning)
            {
                return false;
            }

            if (_isWorldStateDirty)
            {
                OnWorldStateChanged();
                _isWorldStateDirty = false;
            }

            if (_planRequest.HasRequest)
            {
                CompletePlanRequest();
            }
            else if (PlanRunner.State == PlanRunner.RunnerState.None)
            {
                RequestPlanAndComplete(PlanRequestType.Begin);
            }

            if (PlanRunner.State == PlanRunner.RunnerState.Running)
            {
                PlanRunner.Tick();
            }

            switch (PlanRunner.State)
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

            return true;
        }

        private void RequestPlan(PlanRequestType requestType, int targetTaskIndex = 0, int processIndex = 0)
        {
            _planRequest.Request(requestType, PlannerCore.Plan(Domain, WorldState, targetTaskIndex), processIndex);
        }

        private void RequestPlanAndComplete(PlanRequestType requestType, int targetTaskIndex = 0, int processIndex = 0)
        {
            RequestPlan(requestType, targetTaskIndex, processIndex);
            CompletePlanRequest();
        }

        private void CompletePlanRequest()
        {
            if (_planRequest.Complete(out var plan))
            {
                switch (_planRequest.RequestType)
                {
                    case PlanRequestType.Begin:
                        PlanRunner.Begin(Domain, plan, WorldState);
                        break;
                    case PlanRequestType.ReplaceAndResume:
                        PlanRunner.ReplaceAndResumePlan(_planRequest.ProcessIndex, plan);
                        break;
                }

                return;
            }

            if (_planRequest.RequestType == PlanRequestType.ReplaceAndResume)
            {
                var processIndex = _planRequest.ProcessIndex - 1;
                if (processIndex >= 0)
                {
                    var process = PlanRunner.Processes[processIndex];
                    RequestPlanAndComplete(PlanRequestType.ReplaceAndResume, process.Plan.TargetTaskIndex,
                        processIndex);
                    return;
                }
            }

            PlanRunner.Stop();
        }

        private void OnSuccess()
        {
            RequestPlan(PlanRequestType.Begin);
        }

        private void OnFailed()
        {
            var index = PlanRunner.FailedProcessIndex;
            var process = PlanRunner.Processes[index];
            var plan = process.Plan;

            RequestPlanAndComplete(PlanRequestType.ReplaceAndResume, plan.TargetTaskIndex, index);
        }

        private void OnWorldStateChanged()
        {
            switch (PlanRunner.State)
            {
                case PlanRunner.RunnerState.Running:
                    CompareAndRePlan();
                    break;

                default:
                    _planRequest.Request(PlanRequestType.Begin, PlannerCore.Plan(Domain, WorldState));
                    break;
            }
        }

        private void CompareAndRePlan()
        {
            var currentProcesses = PlanRunner.Processes;
            for (var i = 0; i < currentProcesses.Count; i++)
            {
                var process = currentProcesses[i];
                var plan = process.Plan;
                if (!PlannerCore.PlanImmediate(Domain, WorldState, out var newPlan, plan.TargetTaskIndex))
                {
                    PlanRunner.Stop();
                    return;
                }

                switch (CompareMtr(plan.MethodTraversalRecord, newPlan.MethodTraversalRecord))
                {
                    case MtrCompareResult.Same:
                        break;

                    case MtrCompareResult.NewPlanIsBetter:
                        PlanRunner.ReplaceAndResumePlan(i, newPlan);
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
            PlanRunner.Stop();
            Domain.Dispose();
        }
    }
}
