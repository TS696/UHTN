using System.Collections.Generic;

namespace UHTN
{
    public interface IMethod
    {
        List<ConditionToDecompose> PreConditions { get; }

        List<SubTask> SubTasks { get; }
    }
}
