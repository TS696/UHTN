using System;
using UnityEngine;

namespace UHTN.Agent
{
    public abstract class HtnAgentBase : MonoBehaviour
    {
        private enum DomainExecutionType
        {
            RunUntilSuccess,
            RePlanForever
        }

        [SerializeField]
        private DomainExecutionType _executionType;

        public bool IsRunning { get; private set; }

        public Planner Planner { get; private set; }

        public SensorContainer SensorContainer { get; private set; }

        protected void InitializeImpl(Domain domain)
        {
            Planner = new Planner(domain, domain.CreateWorldState());
            SensorContainer = new SensorContainer(Planner.WorldState);
        }

        public void Run()
        {
            ThrowIfNotInitialized();
            SensorContainer.OnPreExecuteDomain();
            Planner.Begin();
            IsRunning = true;
        }

        public void Stop()
        {
            ThrowIfNotInitialized();
            Planner.Stop();
            IsRunning = false;
        }

        private void ThrowIfNotInitialized()
        {
            if (Planner == null)
            {
                throw new InvalidOperationException("HtnAgent is not initialized.");
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
