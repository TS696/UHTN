using System;
using UnityEngine;

namespace UHTN.DomainAssets
{
    [Serializable]
    public class WorldStateField
    {
        [SerializeField]
        private string _name;

        public string Name => _name;

        [SerializeReference]
        private IWsFieldType _fieldType;

        public IWsFieldType FieldType => _fieldType;
        
        [SerializeReference]
        private ISensorCreator _sensorCreator;
        public ISensorCreator SensorCreator => _sensorCreator;
    }
}
