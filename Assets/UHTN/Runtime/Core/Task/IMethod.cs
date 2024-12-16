using System.Collections.Generic;

namespace UHTN
{
    public interface IMethod
    {
        List<ConditionToDecompose> Preconditions { get; }

        List<SubTask> SubTasks { get; }
    }
}
