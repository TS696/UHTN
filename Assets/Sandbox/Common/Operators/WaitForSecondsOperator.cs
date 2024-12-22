using UHTN;
using UnityEngine;

namespace Sandbox.Common
{
    public class WaitForSecondsOperator : IOperator
    {
        private readonly float _duration;
        private float _elapsedTime;

        public WaitForSecondsOperator(float duration)
        {
            _duration = duration;
        }

        public void Begin()
        {
            _elapsedTime = 0;
        }

        public OperatorState Tick()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= _duration)
            {
                return OperatorState.Success;
            }

            return OperatorState.Running;
        }

        public void End()
        {
        }
    }
}
