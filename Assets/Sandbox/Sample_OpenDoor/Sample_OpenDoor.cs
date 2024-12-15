using Sandbox.Common;
using UHTN;
using UHTN.Agent;
using UHTN.Builder;
using UnityEngine;

namespace Sandbox
{
    [RequireComponent(typeof(HtnAgent))]
    public class Sample_OpenDoor : MonoBehaviour
    {
        [SerializeField]
        private Agent _agent;

        [SerializeField]
        private GameObject _door;

        [SerializeField]
        private GameObject _redKey;

        [SerializeField]
        private GameObject _blueKey;

        [SerializeField]
        private GameObject _yellowKey;

        private HtnAgent _htnAgent;

        private Planner _planner;

        private enum WorldState
        {
            [WsFieldHint(typeof(WsFieldBool))]
            HasRedKey,

            [WsFieldHint(typeof(WsFieldBool))]
            HasBlueKey,

            [WsFieldHint(typeof(WsFieldBool))]
            HasYellowKey,

            [WsFieldHint(typeof(WsFieldBool))]
            DoorIsOpen,
        }

        private void Awake()
        {
            _htnAgent = GetComponent<HtnAgent>();
        }

        private void Start()
        {
            var builder = DomainBuilder<WorldState>.Create();

            builder.Root
                .Methods(
                    builder.Method()
                        .SubTasks(
                            builder.Primitive("OpenDoorTask")
                                .Precondition(WorldState.HasRedKey, StateCondition.Equal(true))
                                .Precondition(WorldState.HasBlueKey, StateCondition.Equal(true))
                                .Precondition(WorldState.HasYellowKey, StateCondition.Equal(true))
                                .Effect(WorldState.DoorIsOpen, StateEffect.Assign(true))
                                .Operator(new DoorOpenOperator(_agent, _door))
                        ),
                    builder.Method()
                        .SubTasks(
                            builder.Primitive()
                                .Effect(WorldState.HasRedKey, StateEffect.Assign(true))
                                .Operator(new GetKeyOperator(_agent, _redKey)),
                            builder.Primitive()
                                .Effect(WorldState.HasBlueKey, StateEffect.Assign(true))
                                .Operator(new GetKeyOperator(_agent, _blueKey)),
                            builder.Primitive()
                                .Effect(WorldState.HasYellowKey, StateEffect.Assign(true))
                                .Operator(new GetKeyOperator(_agent, _yellowKey)),
                            builder.CompoundSlot(builder.Root, DecompositionTiming.Delayed)
                        )
                );

            var domain = builder.Resolve();

            _htnAgent.Initialize(domain);

            _htnAgent.AddSensor((int)WorldState.HasRedKey, new GameObjectActiveSensor(_redKey, true));
            _htnAgent.AddSensor((int)WorldState.HasBlueKey, new GameObjectActiveSensor(_blueKey, true));
            _htnAgent.AddSensor((int)WorldState.HasYellowKey, new GameObjectActiveSensor(_yellowKey, true));
            _htnAgent.AddSensor((int)WorldState.DoorIsOpen, new GameObjectActiveSensor(_door, true));

            _htnAgent.Run();
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

        private class DoorOpenOperator : IOperator
        {
            private readonly Agent _agent;
            private readonly GameObject _door;

            public DoorOpenOperator(Agent agent, GameObject door)
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

        private void OnGUI()
        {
            var pos = new Vector2(5, 5);
            var size = new Vector2(100, 20);
            var rect = new Rect(pos, size);

            var hasRed = GUI.Toggle(rect, _redKey.activeInHierarchy, "Red Key");
            rect.y += 30;
            var hasBlue = GUI.Toggle(rect, _blueKey.activeInHierarchy, "Blue Key");
            rect.y += 30;
            var hasYellow = GUI.Toggle(rect, _yellowKey.activeInHierarchy, "Yellow Key");

            if (hasRed != _redKey.activeInHierarchy)
            {
                _redKey.SetActive(hasRed);
            }

            if (hasBlue != _blueKey.activeInHierarchy)
            {
                _blueKey.SetActive(hasBlue);
            }

            if (hasYellow != _yellowKey.activeInHierarchy)
            {
                _yellowKey.SetActive(hasYellow);
            }
        }
    }
}
