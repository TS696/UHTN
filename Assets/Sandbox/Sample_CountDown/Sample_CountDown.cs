using Sandbox.Common;
using UHTN;
using UHTN.Agent;
using UHTN.Builder;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.Sample_CountDown
{
    [RequireComponent(typeof(HtnAgent))]
    public class Sample_CountDown : MonoBehaviour
    {
        [SerializeField]
        private int _count;

        [SerializeField]
        private Text _text;

        private HtnAgent _htnAgent;

        private enum WorldState
        {
            [WsFieldHint(typeof(WsFieldInt))]
            Count
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
                            builder.Primitive().Operator(new WaitForSecondsOperator(1)),
                            builder.Primitive()
                                .Precondition(WorldState.Count, StateCondition.GreaterThan(0))
                                .Effect(WorldState.Count, StateEffect.Subtract(1)),
                            builder.CompoundSlot(builder.Root, DecompositionTiming.Immediate)
                        ),
                    builder.Method()
                );

            var domain = builder.Resolve();
            _htnAgent.Prepare(domain);
            _htnAgent.SensorContainer.AddSensor((int)WorldState.Count, new FixedValueSensor(_count));
            _htnAgent.Planner.WorldState.OnValueChanged += (index, value, _) =>
            {
                if (index == (int)WorldState.Count)
                {
                    _count = value;
                    _text.text = _count.ToString();
                }
            };
            
            _htnAgent.Run();

            _text.text = _count.ToString();
        }

        private class FixedValueSensor : IIntSensor
        {
            public SensorUpdateMode UpdateMode => SensorUpdateMode.PreExecuteDomain;
            private readonly int _value;

            public FixedValueSensor(int value)
            {
                _value = value;
            }

            public int Update(int current)
            {
                return _value;
            }
        }
    }
}
