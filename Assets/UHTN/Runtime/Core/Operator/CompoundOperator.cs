namespace UHTN
{
    public class CompoundOperator : IOperator
    {
        private readonly IOperator[] _operators;
        private readonly OperatorState[] _operatorStates;

        public CompoundOperator(IOperator[] operators)
        {
            _operators = operators;
            _operatorStates = new OperatorState[_operators.Length];
        }

        public void Begin()
        {
            foreach (var op in _operators)
            {
                op.Begin();
            }

            for (var i = 0; i < _operatorStates.Length; i++)
            {
                _operatorStates[i] = OperatorState.Running;
            }
        }

        public OperatorState Tick()
        {
            for (var i = 0; i < _operators.Length; i++)
            {
                if (_operatorStates[i] is OperatorState.Running)
                {
                    _operatorStates[i] = _operators[i].Tick();

                    if (_operatorStates[i] is OperatorState.Failed or OperatorState.Success)
                    {
                        _operators[i].End();
                    }
                }
            }

            var isSuccess = true;
            var isFailed = false;
            for (var i = 0; i < _operators.Length; i++)
            {
                if (_operatorStates[i] == OperatorState.Success)
                {
                    continue;
                }

                if (_operatorStates[i] == OperatorState.Failed)
                {
                    isFailed = true;
                }

                isSuccess = false;
            }

            if (isFailed)
            {
                return OperatorState.Failed;
            }

            if (isSuccess)
            {
                return OperatorState.Success;
            }

            return OperatorState.Running;
        }

        public void End()
        {
            for (var i = 0; i < _operators.Length; i++)
            {
                if (_operatorStates[i] is OperatorState.Running)
                {
                    _operators[i].End();
                }
            }
        }
    }
}
