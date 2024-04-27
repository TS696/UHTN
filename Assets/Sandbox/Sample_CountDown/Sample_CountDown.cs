using UHTN;
using UHTN.Builder;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox
{
    public class Sample_CountDown : MonoBehaviour
    {
        [SerializeField]
        private int _count;

        [SerializeField]
        private Text _text;

        private Planner _planner;

        private enum WorldState
        {
            Count
        }

        void Start()
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

            var (domain, worldState) = builder.Resolve();
            _planner = new Planner(domain, worldState.Value);
            _planner.ExecutionType = PlannerExecutionType.RunUntilSuccess;

            worldState.SetInt(WorldState.Count, _count);
            _text.text = _count.ToString();
            _planner.Begin();
        }

        private void Update()
        {
            _planner.Tick();
        }

        private void OnDestroy()
        {
            _planner.Dispose();
        }
    }
}
