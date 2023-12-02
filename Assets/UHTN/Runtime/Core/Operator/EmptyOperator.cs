namespace UHTN
{
    public class EmptyOperator : IOperator
    {
        public static readonly EmptyOperator Instance = new();

        public void Begin()
        {
        }

        public OperatorState Tick()
        {
            return OperatorState.Success;
        }

        public void End()
        {
        }
    }
}
