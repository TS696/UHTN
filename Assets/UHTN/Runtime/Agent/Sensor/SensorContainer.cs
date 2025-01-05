using System.Collections.Generic;

namespace UHTN.Agent
{
    public class SensorContainer
    {
        public class SensorHolder
        {
            private readonly int _index;
            private readonly ISensor _sensor;
            public SensorUpdateMode UpdateMode => _sensor.UpdateMode;

            public SensorHolder(int index, ISensor sensor)
            {
                _index = index;
                _sensor = sensor;
            }

            public void UpdateState(WorldState worldState)
            {
                var newState = _sensor.UpdateState(worldState.Values[_index]);
                worldState.SetValue(_index, newState);
            }
        }

        private readonly WorldState _worldState;
        private readonly List<SensorHolder> _sensors = new();
        public IReadOnlyList<SensorHolder> Sensors => _sensors;

        public SensorContainer(WorldState worldState)
        {
            _worldState = worldState;
        }

        public void AddSensor(int index, ISensor sensor)
        {
            _sensors.Add(new SensorHolder(index, sensor));
        }

        public void OnPreExecuteDomain()
        {
            UpdateSensors(SensorUpdateMode.PreExecuteDomain);
        }

        public void Tick()
        {
            UpdateSensors(SensorUpdateMode.EveryTick);
        }

        private void UpdateSensors(SensorUpdateMode updateMode)
        {
            foreach (var sensor in _sensors)
            {
                if (sensor.UpdateMode == updateMode)
                {
                    sensor.UpdateState(_worldState);
                }
            }
        }
    }
}
