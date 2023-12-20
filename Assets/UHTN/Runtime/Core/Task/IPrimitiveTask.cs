namespace UHTN
{
    public interface IPrimitiveTask : ITask
    {
        StateCondition[] PreConditions { get; }
        StateEffect[] Effects { get; }
        IOperator Operator { get; }
        void OnPreExecute();
        void OnPostExecute();
    }
}
