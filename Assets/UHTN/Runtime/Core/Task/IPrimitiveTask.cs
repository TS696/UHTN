using System.Collections.Generic;

namespace UHTN
{
    public interface IPrimitiveTask : ITask
    {
        List<ConditionToDecompose> Preconditions { get; }
        List<EffectToDecompose> Effects { get; }
        IOperator Operator { get; }
        void OnPreExecute();
        void OnPostExecute();
    }
}
