namespace UHTN
{
    public interface IOperator
    {
        void Begin();
        OperatorState Tick();
        void End();
    }
}
