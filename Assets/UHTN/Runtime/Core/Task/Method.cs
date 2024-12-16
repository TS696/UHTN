using System.Collections.Generic;

namespace UHTN
{
    public class Method : IMethod
    {
        public List<ConditionToDecompose> PreConditions { get; } = new();

        public List<SubTask> SubTasks { get; } = new();
    }
}
