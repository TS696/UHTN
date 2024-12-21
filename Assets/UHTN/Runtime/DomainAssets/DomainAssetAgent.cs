using UHTN.Agent;
using UnityEngine;

namespace UHTN.DomainAssets
{
    public class DomainAssetAgent : HtnAgentBase
    {
        [SerializeField]
        private DomainAsset _asset;

        public void Initialize(object userData)
        {
            var domain = _asset.ResolveDomain(userData);
            InitializeImpl(domain);
            _asset.ResolveSensor(SensorContainer, userData);
        }
    }
}
