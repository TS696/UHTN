using System;
using UnityEngine;

namespace UHTN.Agent
{
    public class HtnAgent : MonoBehaviour
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

        private SensorContainer _sensorContainer;

        public void Initialize(Domain domain)
        {
            Planner = new Planner(domain, domain.CreateWorldState());
            _sensorContainer = new SensorContainer(Planner.WorldState);
        }

        public void AddSensor(int worldStateIndex, ISensor sensor)
        {
            _sensorContainer.AddSensor(worldStateIndex, sensor);
        }

        public void Run()
        {
            ThrowIfNotInitialized();
            _sensorContainer.OnPreExecuteDomain();
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

            _sensorContainer.Tick();

            if (!Planner.Tick())
            {
                if (_executionType == DomainExecutionType.RunUntilSuccess)
                {
                    IsRunning = false;
                    return;
                }

                _sensorContainer.OnPreExecuteDomain();
                Planner.Begin();
            }
        }

        private void OnDestroy()
        {
            Planner?.Dispose();
        }
    }
}
