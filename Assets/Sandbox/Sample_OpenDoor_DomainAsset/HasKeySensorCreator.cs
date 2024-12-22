using Sandbox.Common;
using System;
using UHTN;
using UHTN.Agent;
using UHTN.DomainAssets;
using UnityEngine;

namespace Sandbox.Sample_OpenDoor_DomainAsset
{
    [SensorCreator("Sample_OpenDoor/HasKey", typeof(WsFieldBool))]
    [Serializable]
    public class HasKeySensorCreator : ISensorCreator
    {
        [SerializeField]
        private Sample_OpenDoor_DomainAsset.KeyType _keyType;

        public ISensor CreateSensor(object userData)
        {
            var context = userData as Sample_OpenDoor_DomainAsset;
            var key = context.GetKey(_keyType);
            return new GameObjectActiveSensor(key, true);
        }
    }
}
