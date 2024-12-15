using Sandbox.Common;
using UHTN;
using UHTN.Agent;
using UHTN.Builder;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox
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
                                .Effect(WorldState.Count, StateEffect.Subtract(1))
                                .Operator(() =>
                                {
                                    _count--;
                                    _text.text = _count.ToString();
                                }),
                            builder.CompoundSlot(builder.Root, DecompositionTiming.Immediate)
                        ),
                    builder.Method()
                );

            var domain = builder.Resolve();
            _htnAgent.Initialize(domain);
            _htnAgent.Run();

            _htnAgent.SetState((int)WorldState.Count, _count);
            _text.text = _count.ToString();
        }
    }
}
