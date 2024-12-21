using Sandbox.Common;
using UHTN;
using UHTN.DomainAssets;
using UnityEngine;

namespace Sandbox.Sample_OpenDoor_DomainAsset
{
    [PrimitiveTaskAsset("Sample_OpenDoor/OpenDoor")]
    public class OpenDoorTaskAsset : PrimitiveTaskAssetBase
    {
        protected override IOperator CreateOperator(object userData)
        {
            var context = userData as Sample_OpenDoor_DomainAsset;
            var agent = context.Agent;
            var door = context.Door;
            return new OpenDoorOperator(agent, door);
        }

        private class OpenDoorOperator : IOperator
        {
            private readonly Agent _agent;
            private readonly GameObject _door;

            public OpenDoorOperator(Agent agent, GameObject door)
            {
                _agent = agent;
                _door = door;
            }

            public void Begin()
            {
                _agent.SetDestination(_door.transform.position);
            }

            public OperatorState Tick()
            {
                if (!_door.activeInHierarchy)
                {
                    return OperatorState.Success;
                }

                if (_agent.IsBusy)
                {
                    return OperatorState.Running;
                }

                _door.SetActive(false);
                return OperatorState.Success;
            }

            public void End()
            {
                _agent.SetDestination(null);
            }
        }
    }
}
