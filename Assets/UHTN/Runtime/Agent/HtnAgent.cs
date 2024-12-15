using System;
using UnityEngine;

namespace UHTN.Agent
{
    public class HtnAgent : MonoBehaviour
    {
        [SerializeField]
        private PlannerExecutionType _executionType;

        public bool IsRunning => Planner.IsRunning;

        public Planner Planner { get; private set; }

        private SensorContainer _sensorContainer;

        public void Initialize(Domain domain)
        {
            Planner = new Planner(domain, domain.CreateWorldState());
            Planner.ExecutionType = _executionType;
            _sensorContainer = new SensorContainer(Planner);
        }

        public void AddSensor(int worldStateIndex, ISensor sensor)
        {
            _sensorContainer.AddSensor(worldStateIndex, sensor);
        }

        public void SetState(int worldStateIndex, int value)
        {
            Planner.WorldState.SetValue(worldStateIndex, value);
        }

        private void OnValidate()
        {
            if (Planner == null)
            {
                return;
            }

            Planner.ExecutionType = _executionType;
        }

        public void Run()
        {
            ThrowIfNotInitialized();
            Planner.Begin();
        }

        public void Stop()
        {
            ThrowIfNotInitialized();
            Planner.Stop();
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
            if (Planner == null || !Planner.IsRunning)
            {
                return;
            }

            _sensorContainer?.Tick();
            Planner?.Tick();
        }

        private void OnDestroy()
        {
            Planner?.Dispose();
        }
    }
}
