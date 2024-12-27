using System;
using UnityEngine;

namespace UHTN.Agent
{
    public enum DomainExecutionType
    {
        RunUntilSuccess,
        RePlanForever
    }

    public abstract class HtnAgentBase : MonoBehaviour
    {
        [SerializeField]
        private DomainExecutionType _executionType;

        public DomainExecutionType ExecutionType
        {
            get => _executionType;
            set => _executionType = value;
        }

        public bool IsRunning { get; private set; }

        public Planner Planner { get; private set; }

        public SensorContainer SensorContainer { get; private set; }

        protected void PrepareImpl(Domain domain)
        {
            Planner?.Dispose();
            
            Planner = new Planner(domain, domain.CreateWorldState());
            SensorContainer = new SensorContainer(Planner.WorldState);
        }

        public void Run()
        {
            ThrowIfNotPrepared();
            SensorContainer.OnPreExecuteDomain();
            Planner.Begin();
            IsRunning = true;
        }

        public void Stop()
        {
            ThrowIfNotPrepared();
            Planner.Stop();
            IsRunning = false;
        }

        private void ThrowIfNotPrepared()
        {
            if (Planner == null)
            {
                throw new InvalidOperationException("HtnAgent is not prepared.");
            }
        }

        private void OnDisable()
        {
            Planner?.Pause();
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            SensorContainer.Tick();

            if (!Planner.Tick())
            {
                if (_executionType == DomainExecutionType.RunUntilSuccess)
                {
                    IsRunning = false;
                    return;
                }

                SensorContainer.OnPreExecuteDomain();
                Planner.Begin();
            }
        }

        private void OnDestroy()
        {
            Planner?.Dispose();
        }
    }
}
