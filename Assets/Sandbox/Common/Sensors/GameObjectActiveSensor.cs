using UHTN.Agent;
using UnityEngine;

namespace Sandbox.Common
{
    public class GameObjectActiveSensor : IBoolSensor
    {
        private readonly GameObject _gameObject;
        private readonly bool _invert;

        public GameObjectActiveSensor(GameObject gameObject, bool invert = false)
        {
            _gameObject = gameObject;
            _invert = invert;
        }

        public bool Update(bool current)
        {
            return _invert ? !_gameObject.activeInHierarchy : _gameObject.activeInHierarchy;
        }
    }
}
