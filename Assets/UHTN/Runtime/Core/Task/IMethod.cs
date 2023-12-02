using System.Collections.Generic;

namespace UHTN
{
    public interface IMethod
    {
        StateCondition[] PreConditions { get; }

        List<ITask> SubTasks { get; }
    }
}
