using UHTN.Agent;
using UnityEngine;

namespace UHTN.DomainAssets
{
    public class DomainAssetAgent : HtnAgentBase
    {
        [SerializeField]
        private DomainAsset _asset;

        public DomainAsset Asset
        {
            get => _asset;
            set => _asset = value;
        }

        public void Prepare(object userData)
        {
            var domain = _asset.ResolveDomain(userData);
            PrepareImpl(domain);
            _asset.ResolveSensor(SensorContainer, userData);
        }
    }
}
