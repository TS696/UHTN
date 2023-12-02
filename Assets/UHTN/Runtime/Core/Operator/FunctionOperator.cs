using System;

namespace UHTN
{
    public class FunctionOperator : IOperator
    {
        private readonly Action _begin;
        private readonly Func<OperatorState> _tick;
        private readonly Action _end;

        public FunctionOperator(Action begin, Func<OperatorState> tick, Action end)
        {
            _begin = begin;
            _tick = tick;
            _end = end;
        }

        void IOperator.Begin()
        {
            _begin?.Invoke();
        }

        OperatorState IOperator.Tick()
        {
            if (_tick != null)
            {
                return _tick.Invoke();
            }

            return OperatorState.Success;
        }

        void IOperator.End()
        {
            _end?.Invoke();
        }
    }
}
