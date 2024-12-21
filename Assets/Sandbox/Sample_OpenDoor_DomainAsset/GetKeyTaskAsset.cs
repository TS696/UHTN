using Sandbox.Common;
using UHTN;
using UHTN.DomainAssets;
using UnityEngine;

namespace Sandbox.Sample_OpenDoor_DomainAsset
{
    [PrimitiveTaskAsset("Sample_OpenDoor/GetKey")]
    public class GetKeyTaskAsset : PrimitiveTaskAssetBase
    {
        [SerializeField]
        private Sample_OpenDoor_DomainAsset.KeyType _keyType;

        protected override IOperator CreateOperator(object userData)
        {
            var context = userData as Sample_OpenDoor_DomainAsset;
            var key = context.GetKey(_keyType);
            return new GetKeyOperator(context.Agent, key);
        }

        private class GetKeyOperator : IOperator
        {
            private readonly Agent _agent;
            private readonly GameObject _key;

            public GetKeyOperator(Agent agent, GameObject key)
            {
                _agent = agent;
                _key = key;
            }

            public void Begin()
            {
                _agent.SetDestination(_key.transform.position);
            }

            public OperatorState Tick()
            {
                if (!_key.activeInHierarchy)
                {
                    return OperatorState.Success;
                }

                if (_agent.IsBusy)
                {
                    return OperatorState.Running;
                }

                _key.SetActive(false);
                return OperatorState.Success;
            }

            public void End()
            {
                _agent.SetDestination(null);
            }
        }
    }
}
