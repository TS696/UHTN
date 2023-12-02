using System;

namespace UHTN
{
    public class WorldStateDescription
    {
        public int StateLength => _stateNames.Length;

        public string GetStateName(int index)
        {
            return _stateNames[index];
        }

        private readonly string[] _stateNames;

        public static WorldStateDescription Create<T>() where T : Enum
        {
            var names = Enum.GetNames(typeof(T));
            return new WorldStateDescription(names);
        }

        public WorldStateDescription(string[] stateNames)
        {
            _stateNames = stateNames;
        }
    }
}
