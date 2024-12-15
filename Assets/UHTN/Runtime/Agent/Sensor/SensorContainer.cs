namespace UHTN.Agent
{
    public class SensorContainer
    {
        private readonly ISensor[] _sensors;
        private readonly WorldState _worldState;

        public SensorContainer(Planner planner)
        {
            _worldState = planner.WorldState;
            _sensors = new ISensor[_worldState.StateLength];
        }

        public void AddSensor(int index, ISensor sensor)
        {
            _sensors[index] = sensor;
        }

        public void Tick()
        {
            for (var i = 0; i < _sensors.Length; i++)
            {
                UpdateSensor(i);
            }
        }

        private void UpdateSensor(int index)
        {
            var sensor = _sensors[index];
            if (sensor == null)
            {
                return;
            }
            
            var newState = sensor.UpdateState(_worldState.Values[index]);
            _worldState.SetValue(index, newState);
        }
    }
}
