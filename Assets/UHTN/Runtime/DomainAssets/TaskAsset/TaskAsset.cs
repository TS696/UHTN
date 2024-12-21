using UnityEngine;

namespace UHTN.DomainAssets
{
    public abstract class TaskAsset : ScriptableObject, IDomainAssetContent
    {
        [SerializeField, HideInInspector]
        private DomainAsset _domain;

        public DomainAsset Domain => _domain;

        public void SetDomain(DomainAsset domain)
        {
            _domain = domain;
        }

        public abstract void Resolve(DomainAsset.DomainAssetResolver resolver);
    }
}
